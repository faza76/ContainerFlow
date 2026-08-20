<script setup lang="ts">
import { useAuthStore } from '@/stores/auth'
import { useRouter } from 'vue-router'
import { computed, ref } from 'vue'

const auth = useAuthStore()
const router = useRouter()
const isMobileMenuOpen = ref(false)

const navItems = computed(() => {
  const items = [
    { label: 'Dashboard', icon: '📊', route: '/dashboard' },
  ]
  if (auth.canManageBookings || auth.isCustomer) {
    items.push({ label: 'Bookings', icon: '📦', route: '/bookings' })
  }
  if (auth.canManageContainers) {
    items.push({ label: 'Containers', icon: '🚢', route: '/containers' })
  }
  items.push({ label: 'Notifications', icon: '🔔', route: '/notifications' })
  return items
})

function logout() {
  auth.logout()
  router.push('/login')
}
</script>

<template>
  <div class="flex h-screen bg-gray-50">
    <!-- Sidebar -->
    <aside class="hidden md:flex flex-col w-64 bg-white border-r border-gray-200">
      <div class="p-4 border-b border-gray-200">
        <h1 class="text-lg font-bold text-primary-700">ContainerFlow</h1>
        <p class="text-xs text-gray-500 mt-1">Container Tracking System</p>
      </div>
      <nav class="flex-1 p-3 space-y-1">
        <router-link
          v-for="item in navItems"
          :key="item.route"
          :to="item.route"
          class="flex items-center gap-3 px-3 py-2 rounded-lg text-sm font-medium transition-colors"
          :class="router.currentRoute.value.path.startsWith(item.route) ? 'bg-primary-50 text-primary-700' : 'text-gray-600 hover:bg-gray-100'"
        >
          <span>{{ item.icon }}</span>
          <span>{{ item.label }}</span>
        </router-link>
      </nav>
      <div class="p-4 border-t border-gray-200">
        <div class="text-sm text-gray-700">{{ auth.user?.name }}</div>
        <div class="text-xs text-gray-400 mb-2 capitalize">{{ auth.user?.role }}</div>
        <button @click="logout" class="text-xs text-red-600 hover:text-red-800">Sign out</button>
      </div>
    </aside>

    <!-- Mobile header -->
    <div class="md:hidden fixed top-0 left-0 right-0 z-50 bg-white border-b border-gray-200 px-4 py-3 flex items-center justify-between">
      <h1 class="text-lg font-bold text-primary-700">ContainerFlow</h1>
      <button @click="isMobileMenuOpen = !isMobileMenuOpen" class="text-gray-600 text-2xl">☰</button>
    </div>

    <!-- Mobile menu overlay -->
    <div v-if="isMobileMenuOpen" class="md:hidden fixed inset-0 z-40 bg-black/30" @click="isMobileMenuOpen = false"></div>
    <aside v-if="isMobileMenuOpen" class="md:hidden fixed top-0 left-0 z-50 h-full w-64 bg-white shadow-lg">
      <div class="p-4 border-b border-gray-200 flex justify-between items-center">
        <h1 class="text-lg font-bold text-primary-700">ContainerFlow</h1>
        <button @click="isMobileMenuOpen = false" class="text-gray-500 text-xl">✕</button>
      </div>
      <nav class="p-3 space-y-1">
        <router-link
          v-for="item in navItems"
          :key="item.route"
          :to="item.route"
          @click="isMobileMenuOpen = false"
          class="flex items-center gap-3 px-3 py-2 rounded-lg text-sm font-medium"
          :class="router.currentRoute.value.path.startsWith(item.route) ? 'bg-primary-50 text-primary-700' : 'text-gray-600'"
        >
          <span>{{ item.icon }}</span>
          <span>{{ item.label }}</span>
        </router-link>
        <hr class="my-2" />
        <button @click="logout" class="flex items-center gap-3 px-3 py-2 text-sm text-red-600 w-full">Sign out</button>
      </nav>
    </aside>

    <!-- Main content -->
    <main class="flex-1 overflow-auto pt-14 md:pt-0">
      <router-view />
    </main>
  </div>
</template>