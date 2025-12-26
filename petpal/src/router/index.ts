import { createRouter, createWebHistory } from 'vue-router'
import Layout from '../components/Layout.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/about',
      name: 'about',
      // route level code-splitting
      // this generates a separate chunk (About.[hash].js) for this route
      // which is lazy-loaded when the route is visited.
      component: () => import('../views/AboutView.vue'),
    },
    {
      path: '/',
      name: 'layout',
      component: Layout,
      children: [
        {
          path: '/init',
          name: 'init',
          component: () => import('../components/Home.vue'),
        },
        {
          path: '/about',
          name: 'about',
          component: () => import('../views/AboutView.vue'),
        },
         {
          path: '/publish',
          name: 'publish',
          component: () => import('@/views/PublishRequirement.vue'),
        },
        {
          path: '/accept',
          name: 'accept',
          component: () => import('@/views/AcceptRequirement.vue'),
        },
        {
          path: '/manage',
          name: 'manage',
          component: () => import('@/views/ManageCommunity.vue'),
        },
        {
          path: '/',
          redirect: '/login'
        },
        {
          path: '/login',
          name: 'login',
          component: () => import('@/views/auth/LoginView.vue'),
          meta: { requiresGuest: true }
        },
        {
          path: '/register',
          name: 'register',
          component: () => import('@/views/auth/RegisterView.vue'),
          meta: { requiresGuest: true }
        },
        {
        path: '/profile',
        name: 'Profile',
        component: () => import('@/views/Profile.vue'),
        meta: { requiresAuth: true } // 需要登录
       },
      ]
    },
    
  ],
})

// // 路由守卫
// router.beforeEach((to, from, next) => {
//   const isAuthenticated = !!localStorage.getItem('auth_token')
  
//   if (to.meta.requiresAuth && !isAuthenticated) {
//     // 需要登录但未登录，跳转到登录页
//     next('/login')
//   } else if (to.meta.requiresGuest && isAuthenticated) {
//     // 需要未登录但已登录，跳转到首页
//     next('/init')
//   } else {
//     next()
//   }
// })

export default router
