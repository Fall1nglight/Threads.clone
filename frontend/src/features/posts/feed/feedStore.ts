import type { PagedRequest, PagedResponse } from '@/shared/types/paginationTypes.ts'
import type { CreatePostDto, PostDto } from '@/features/posts/postTypes.ts'
import { defineStore } from 'pinia'
import { reactive } from 'vue'
import feedApi from '@/features/posts/feed/feedApi.ts'
import postApi from '@/features/posts/postApi.ts'
import type { FeedKind } from '@/features/posts/feed/feedTypes.ts'
import likeApi from '@/features/posts/likeApi.ts'

export type FeedStatus = 'idle' | 'loading' | 'success' | 'error'

export type FeedState = {
  items: PostDto[]
  hasMore: boolean
  cursor: string | null
  status: FeedStatus
  error: unknown
}

function createFeedState(): FeedState {
  return {
    items: [],
    hasMore: false,
    cursor: null,
    status: 'idle',
    error: null,
  }
}

export const useFeedStore = defineStore('feed', () => {
  const feeds = reactive<Record<FeedKind, FeedState>>({
    anonymous: createFeedState(),
    global: createFeedState(),
    personal: createFeedState(),
  })

  async function fetchInitialFeed(kind: FeedKind) {
    const feed = feeds[kind]

    // prevent simultaneous calls from executing
    if (feed.status == 'loading') return

    feed.status = 'loading'
    feed.items = []
    feed.hasMore = false
    feed.cursor = null
    feed.error = null

    const pagedRequest: PagedRequest = {
      pageSize: 5,
    }

    try {
      const response = await feedApi.getFeed(kind, pagedRequest)
      feed.items = response.items
      feed.hasMore = response.hasMore
      feed.cursor = response.cursor ?? null
      feed.status = 'success'
    } catch (err) {
      feed.status = 'error'
      feed.error = err
      console.error(err)
    }
  }

  async function fetchMoreFeed(kind: FeedKind) {
    const feed = feeds[kind]

    if (!feed.hasMore) return
    if (!feed.cursor) return
    if (feed.status == 'loading') return

    feed.status = 'loading'
    feed.error = null

    const pagedRequest: PagedRequest = {
      pageSize: 5,
      cursor: feed.cursor,
    }

    try {
      const response = await feedApi.getFeed(kind, pagedRequest)
      feed.items.push(...response.items)
      feed.hasMore = response.hasMore
      feed.cursor = response.cursor ?? null
      feed.status = 'success'
    } catch (err) {
      feed.status = 'error'
      feed.error = err
      console.error(err)
    }
  }

  async function createPost(payload: CreatePostDto) {
    const newPost = await postApi.addPost(payload)
    addPostTo('global', newPost)
    addPostTo('personal', newPost)
  }

  function addPostTo(kind: FeedKind, newPost: PostDto) {
    const feed = feeds[kind]

    if (feed.items.some((p) => p.id === newPost.id)) return

    feed.items.unshift(newPost)
  }

  const pendingLikeIds = reactive(new Set<string>())

  async function setLike(postId: string, nextLikeState: boolean) {
    if (pendingLikeIds.has(postId)) return

    pendingLikeIds.add(postId)

    try {
      if (nextLikeState) {
        await likeApi.likePost(postId)
      } else {
        await likeApi.unlikePost(postId)
      }

      updatePostLikeState(postId, nextLikeState)
    } catch (err) {
      console.error(err)
    } finally {
      pendingLikeIds.delete(postId)
    }
  }

  function updatePostLikeState(id: string, nextLikeState: boolean) {
    const authenticatedFeeds: FeedKind[] = ['global', 'personal']

    for (const feedKind of authenticatedFeeds) {
      const feed = feeds[feedKind]
      const post = feed.items.find((post) => post.id === id)

      if (!post) continue
      if (post.isLikedByCurrentUser == nextLikeState) continue

      post.isLikedByCurrentUser = nextLikeState
      post.likeCount = Math.max(0, post.likeCount + (nextLikeState ? 1 : -1))
    }
  }

  async function deletePost(postId: string) {
    await postApi.deletePost(postId)

    const authenticatedFeeds: FeedKind[] = ['global', 'personal']

    for (const feedKind of authenticatedFeeds) {
      const feed = feeds[feedKind]
      feed.items = feed.items.filter((post) => post.id !== postId)
    }
  }

  return { feeds, fetchInitialFeed, fetchMoreFeed, createPost, setLike, deletePost }
})
