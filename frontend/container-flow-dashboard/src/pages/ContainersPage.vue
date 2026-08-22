<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useAuthStore } from '@/stores/auth'
import api from '@/api/client'

const auth = useAuthStore()
const containers = ref<any[]>([])
const loading = ref(true)
const error = ref('')
const statusFilter = ref('')

const statusColors: Record<string, string> = {
  available: 'bg-green-100 text-green-800',
  allocated: 'bg-blue-100 text-blue-800',
  in_yard: 'bg-yellow-100 text-yellow-800',
  loaded: 'bg-purple-100 text-purple-800',
  in_transit: 'bg-indigo-100 text-indigo-800',
  discharged: 'bg-orange-100 text-orange-800',
  delivered: 'bg-teal-100 text-teal-800'
}

function statusClass(s: string) {
  return statusColors[s?.toLowerCase()] || 'bg-gray-100 text-gray-800'
}

const filteredContainers = computed(() => {
  if (!statusFilter.value) return containers.value
  return containers.value.filter(c => (c.status || '').toLowerCase() === statusFilter.value.toLowerCase())
})

async function fetchContainers() {
  try {
    const res = await api.get('/api/containers')
    containers.value = res.data
  } catch (e: any) {
    error.value = 'Failed to load containers'
  } finally {
    loading.value = false
  }
}

import { computed } from 'vue'

onMounted(fetchContainers)
</script>

<template>
  <div class="p-6">
    <div class="flex items-center justify-between mb-6">
      <h2 class="text-2xl font-bold text-gray-800">Containers</h2>
      <select v-model="statusFilter" class="text-sm border border-gray-300 rounded-lg px-3 py-1.5 outline-none focus:ring-2 focus:ring-primary-500">
        <option value="">All Statuses</option>
        <option value="available">Available</option>
        <option value="allocated">Allocated</option>
        <option value="in_yard">In Yard</option>
        <option value="loaded">Loaded</option>
        <option value="in_transit">In Transit</option>
        <option value="discharged">Discharged</option>
        <option value="delivered">Delivered</option>
      </select>
    </div>

    <div v-if="loading" class="flex items-center justify-center h-64">
      <div class="animate-spin w-8 h-8 border-4 border-primary-500 border-t-transparent rounded-full"></div>
    </div>

    <div v-else-if="error" class="bg-red-50 border border-red-200 text-red-700 rounded-lg p-4 text-sm">{{ error }}</div>

    <div v-else-if="filteredContainers.length === 0" class="text-center py-16 text-gray-400">
      <p class="text-lg">No containers found.</p>
    </div>

    <div v-else class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
      <div v-for="c in filteredContainers" :key="c.id"
        class="bg-white rounded-xl shadow-sm border border-gray-100 p-4 hover:shadow-md transition-shadow">
        <div class="flex items-start justify-between mb-3">
          <h3 class="font-semibold text-gray-800">{{ c.containerNumber || c.id?.slice(0, 8) }}</h3>
          <span class="px-2 py-0.5 rounded-full text-xs font-medium" :class="statusClass(c.status)">{{ c.status }}</span>
        </div>
        <div class="text-xs text-gray-500 space-y-1">
          <p>Type: {{ c.containerType || '—' }}</p>
          <p>TEU: {{ c.teuCapacity || '—' }}</p>
          <p>Location: {{ c.currentLocation || '—' }}</p>
        </div>
      </div>
    </div>
  </div>
</template>