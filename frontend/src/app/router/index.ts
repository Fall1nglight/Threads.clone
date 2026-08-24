import { createRouter, createWebHistory } from 'vue-router'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      component: () => import('@/app/layouts/AppShell.vue'),
      children: [
        {
          path: '',
          name: 'home',
          component: () => import('@/features/posts/views/HomeFeedView.vue'),
        },
        {
          path: 'search',
          name: 'search',
          component: () => import('@/features/search/views/SearchView.vue'),
        },
        {
          path: 'activity',
          name: 'activity',
          component: () => import('@/features/activity/views/ActivityView.vue'),
        },
        {
          path: 'users/:userId',
          name: 'user-profile',
          component: () => import('@/features/users/views/ProfileView.vue'),
        },
        {
          path: 'users/:userId/followers',
          name: 'user-followers',
          component: () => import('@/features/follow/views/FollowListView.vue'),
          props: { listType: 'followers' },
        },
        {
          path: 'users/:userId/following',
          name: 'user-following',
          component: () => import('@/features/follow/views/FollowListView.vue'),
          props: { listType: 'following' },
        },
        {
          path: 'blocked-users',
          name: 'blocked-users',
          component: () => import('@/features/block/views/BlockedUsersView.vue'),
        },
        {
          path: 'posts/:postId',
          name: 'post-detail',
          component: () => import('@/features/posts/views/PostDetailView.vue'),
        },
        {
          path: 'posts/:postId/edit',
          name: 'post-edit',
          component: () => import('@/features/posts/views/PostEditView.vue'),
        },
      ],
    },

    {
      path: '/auth',
      redirect: '/auth/login',
      component: () => import('@/app/layouts/AuthShell.vue'),
      children: [
        {
          path: 'login',
          name: 'login',
          component: () => import('@/features/auth/views/LoginView.vue'),
        },
        {
          path: 'signup',
          name: 'signup',
          component: () => import('@/features/auth/views/SignupView.vue'),
        },
      ],
    },

    {
      path: '/:pathMatch(.*)*',
      name: 'not-found',
      component: () => import('@/app/views/NotFoundView.vue'),
    },
  ],
})

export default router
