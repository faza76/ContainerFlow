<script setup lang="ts">
import { ref, onMounted } from 'vue'
import api from '@/api/client'

const notifications = ref<any[]>([])
const loading = ref(true)
const error = ref('')

async function fetchNotifications() {
  try {
    const res = await api.get('/api/notifications')
    notifications.value = res.data
  } catch (e: any) {
    error.value = 'Failed to load notifications'
  } finally {
    loading.value = false
  }
}

function formatTime(ts: string) {
  return new Date(ts).toLocaleString()
}

onMounted(fetchNotifications)
</script>

<template>
  <div class="p-6">
    <h2 class="text-2xl font-bold text-gray-800 mb-6">Notifications</h2>

    <div v-if="loading" class="flex items-center justify-center h-64">
      <div class="animate-spin w-8 h-8 border-4 border-primary-500 border-t-transparent rounded-full"></div>
    </div>

    <div v-else-if="error" class="bg-red-50 border border-red-200 text-red-700 rounded-lg p-4 text-sm">{{ error }}</div>

    <div v-else-if="notifications.length === 0" class="text-center py-16 text-gray-400">
      <p class="text-lg">No notifications yet.</p>
      <p class="text-sm mt-1">Notifications appear when bookings are created, containers are allocated, and shipments move.</p>
    </div>

    <div v-else class="space-y-3">
      <div v-for="n in notifications" :key="n.id"
        class="bg-white rounded-xl shadow-sm border border-gray-100 p-4 hover:shadow-md transition-shadow">
        <div class="flex items-start gap-3">
          <span class="text-xl mt-0.5">🔔</span>
          <div class="flex-1 min-w-0">
            <p class="font-medium text-gray-800">{{ n.title || 'Notification' }}</p>
            <p class="text-sm text-gray-600 mt-0.5">{{ n.message }}</p>
            <div class="flex items-center gap-3 mt-2 text-xs text-gray-400">
              <span>{{ formatTime(n.createdAt || n.timestamp) }}</span>
              <span v-if="n.type" class="px-2 py-0.5 bg-gray-100 rounded-full">{{ n.type }}</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>