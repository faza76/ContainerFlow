<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import api from '@/api/client'

const auth = useAuthStore()
const router = useRouter()
const bookings = ref<any[]>([])
const loading = ref(true)
const error = ref('')

const statusColors: Record<string, string> = {
  pending: 'bg-yellow-100 text-yellow-800',
  confirmed: 'bg-blue-100 text-blue-800',
  in_progress: 'bg-purple-100 text-purple-800',
  completed: 'bg-green-100 text-green-800',
  cancelled: 'bg-red-100 text-red-800'
}

function statusClass(s: string) {
  return statusColors[s?.toLowerCase()] || 'bg-gray-100 text-gray-800'
}

function viewDetail(id: string) {
  router.push(`/bookings/${id}`)
}

async function fetchBookings() {
  try {
    const res = await api.get('/api/bookings')
    bookings.value = res.data
  } catch (e: any) {
    error.value = 'Failed to load bookings'
  } finally {
    loading.value = false
  }
}

onMounted(fetchBookings)
</script>

<template>
  <div class="p-6">
    <div class="flex items-center justify-between mb-6">
      <h2 class="text-2xl font-bold text-gray-800">Bookings</h2>
    </div>

    <div v-if="loading" class="flex items-center justify-center h-64">
      <div class="animate-spin w-8 h-8 border-4 border-primary-500 border-t-transparent rounded-full"></div>
    </div>

    <div v-else-if="error" class="bg-red-50 border border-red-200 text-red-700 rounded-lg p-4 text-sm">{{ error }}</div>

    <div v-else-if="bookings.length === 0" class="text-center py-16 text-gray-400">
      <p class="text-lg">No bookings found.</p>
    </div>

    <div v-else class="overflow-x-auto bg-white rounded-xl shadow-sm border border-gray-100">
      <table class="min-w-full text-sm">
        <thead>
          <tr class="bg-gray-50 text-left text-xs text-gray-500 uppercase tracking-wider">
            <th class="px-4 py-3">Booking #</th>
            <th class="px-4 py-3">Customer</th>
            <th class="px-4 py-3">Container</th>
            <th class="px-4 py-3">Status</th>
            <th class="px-4 py-3">Created</th>
            <th class="px-4 py-3"></th>
          </tr>
        </thead>
        <tbody class="divide-y divide-gray-100">
          <tr v-for="b in bookings" :key="b.id" class="hover:bg-gray-50">
            <td class="px-4 py-3 font-medium">{{ b.bookingNumber || b.id?.slice(0, 8) }}</td>
            <td class="px-4 py-3">{{ b.customerName || b.customerId?.slice(0, 8) }}</td>
            <td class="px-4 py-3">{{ b.containerType || '—' }}</td>
            <td class="px-4 py-3">
              <span class="inline-block px-2 py-0.5 rounded-full text-xs font-medium" :class="statusClass(b.status || b.bookingStatus)">
                {{ b.status || b.bookingStatus || 'Unknown' }}
              </span>
            </td>
            <td class="px-4 py-3 text-gray-400 text-xs">{{ b.createdAt ? new Date(b.createdAt).toLocaleDateString() : '—' }}</td>
            <td class="px-4 py-3">
              <button @click="viewDetail(b.id)" class="text-primary-600 hover:text-primary-800 text-xs font-medium">View</button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>