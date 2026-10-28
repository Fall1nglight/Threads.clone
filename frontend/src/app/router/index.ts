import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/features/auth/stores/authStore.ts'
import { storeToRefs } from 'pinia'
import { useModalStore } from '@/shared/modal/modalStore.ts'

const metaOptions = {
  requiresAuth: 'requiresAuth',
  guestOnly: 'guestOnly',
  showLoginPrompt: 'showLoginPrompt',
}

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  scrollBehavior(to, _from, savedPosition) {
    if (
      to.name === 'home' ||
      to.name === 'post-detail' ||
      to.name === 'user-profile' ||
      to.name === 'profile-settings' ||
      to.name === 'profile-details' ||
      to.name === 'change-password' ||
      to.name === 'user-followers' ||
      to.name === 'user-following' ||
      to.name === 'blocked-users'
    ) {
      return { left: 0, top: 0, behavior: 'instant' }
    }

    return savedPosition ?? false
  },
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
          meta: {
            [metaOptions.requiresAuth]: true,
            [metaOptions.showLoginPrompt]: true,
          },
        },
        {
          path: 'users/:userId',
          name: 'user-profile',
          component: () => import('@/features/users/views/ProfileView.vue'),
        },
        {
          path: 'settings',
          component: () => import('@/features/settings/views/SettingsView.vue'),
          meta: {
            [metaOptions.requiresAuth]: true,
          },
          children: [
            {
              path: '',
              name: 'profile-details',
              component: () => import('@/features/settings/views/ProfileDetailsView.vue'),
            },
            {
              path: 'profile',
              name: 'profile-settings',
              component: () => import('@/features/settings/views/ProfileSettingsView.vue'),
            },
            {
              path: 'change-password',
              name: 'change-password',
              component: () => import('@/features/settings/views/ChangePasswordView.vue'),
            },
          ],
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
          meta: {
            [metaOptions.requiresAuth]: true,
          },
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
          meta: {
            [metaOptions.requiresAuth]: true,
          },
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
          meta: {
            [metaOptions.guestOnly]: true,
          },
        },
        {
          path: 'signup',
          name: 'signup',
          component: () => import('@/features/auth/views/SignupView.vue'),
          meta: {
            [metaOptions.guestOnly]: true,
          },
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

router.beforeEach(async (to, from) => {
  const authStore = useAuthStore()
  const modalStore = useModalStore()
  const { isLoggedIn } = storeToRefs(authStore)

  // this makes sure the auth-flow is completed
  await authStore.ensureSessionReady()

  if (to.meta[metaOptions.requiresAuth] === true && !isLoggedIn.value) {
    if (to.meta[metaOptions.showLoginPrompt] === true) {
      modalStore.openLoginForm()
      return false
    }

    return { name: 'login' }
  }

  if (to.meta[metaOptions.guestOnly] === true && isLoggedIn.value) {
    return { name: from.name ?? 'home' }
  }
})

export default router
