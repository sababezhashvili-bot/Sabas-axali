import { createRouter, createWebHistory } from 'vue-router'

const routes = [
  {
    path: '/',
    component: () => import('../views/MapView.vue')
  },
  {
    path: '/admin',
    component: () => import('../views/AdminView.vue'),
    beforeEnter: (to, from, next) => {
      if (!localStorage.getItem('authToken')) {
        next('/')
      } else {
        next()
      }
    }
  },
  {
    path: '/dashboard',
    component: () => import('../views/DashboardView.vue'),
    beforeEnter: (to, from, next) => {
      if (!localStorage.getItem('authToken')) {
        next('/')
      } else {
        next()
      }
    }
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

export default router
