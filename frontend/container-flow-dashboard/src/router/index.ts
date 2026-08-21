import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import LoginPage from '@/pages/LoginPage.vue'
import Layout from '@/components/Layout.vue'
import DashboardPage from '@/pages/DashboardPage.vue'
import BookingsPage from '@/pages/BookingsPage.vue'
import BookingDetailPage from '@/pages/BookingDetailPage.vue'
import ContainersPage from '@/pages/ContainersPage.vue'
import NotificationsPage from '@/pages/NotificationsPage.vue'

const routes = [
  { path: '/login', name: 'login', component: LoginPage },
  {
    path: '/',
    component: Layout,
    meta: { requiresAuth: true },
    children: [
      { path: '', redirect: '/dashboard' },
      { path: 'dashboard', name: 'dashboard', component: DashboardPage },
      { path: 'bookings', name: 'bookings', component: BookingsPage },
      { path: 'bookings/:id', name: 'booking-detail', component: BookingDetailPage, props: true },
      { path: 'containers', name: 'containers', component: ContainersPage },
      { path: 'notifications', name: 'notifications', component: NotificationsPage }
    ]
  }
]

const router = createRouter({ history: createWebHistory(), routes })

router.beforeEach((to) => {
  const auth = useAuthStore()
  if (to.meta.requiresAuth && !auth.isAuthenticated && to.name !== 'login') {
    return '/login'
  }
})

export default router