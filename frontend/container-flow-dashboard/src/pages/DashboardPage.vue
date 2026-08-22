<script setup lang="ts">
import { ref, onMounted } from 'vue'
import api from '@/api/client'
import { Chart as ChartJS, ArcElement, Tooltip, Legend, CategoryScale, LinearScale, BarElement, PointElement, LineElement, Title } from 'chart.js'
import { Doughnut, Line } from 'vue-chartjs'

ChartJS.register(ArcElement, Tooltip, Legend, CategoryScale, LinearScale, BarElement, PointElement, LineElement, Title)

interface Utilization {
  totalCapacityTeu: number
  allocatedTeu: number
  availableTeu: number
  utilizationPercent: number
}

interface BookingSummary {
  total: number
  pending: number
  confirmed: number
  inProgress: number
  completed: number
  cancelled: number
}

interface ContainerSummary {
  total: number
  available: number
  allocated: number
  inTransit: number
}

interface NotificationSummary {
  total: number
  unread: number
}

const loading = ref(true)
const error = ref('')
const utilization = ref<Utilization>({ totalCapacityTeu: 0, allocatedTeu: 0, availableTeu: 0, utilizationPercent: 0 })
const bookings = ref<BookingSummary>({ total: 0, pending: 0, confirmed: 0, inProgress: 0, completed: 0, cancelled: 0 })
const containers = ref<ContainerSummary>({ total: 0, available: 0, allocated: 0, inTransit: 0 })
const notifications = ref<NotificationSummary>({ total: 0, unread: 0 })
const recentNotifications = ref<any[]>([])
const bookingChartData = ref<any>(null)
const statusChartData = ref<any>(null)
const chartOptions = { responsive: true, maintainAspectRatio: false }

async function fetchDashboard() {
  loading.value = true
  error.value = ''
  try {
    const [utilRes, bookingRes, containerRes, notifRes] = await Promise.allSettled([
      api.get('/api/containers/utilization'),
      api.get('/api/bookings'),
      api.get('/api/containers'),
      api.get('/api/notifications')
    ])

    if (utilRes.status === 'fulfilled') utilization.value = utilRes.value.data

    if (bookingRes.status === 'fulfilled') {
      const data = bookingRes.value.data as any[]
      const counts = { total: data.length, pending: 0, confirmed: 0, inProgress: 0, completed: 0, cancelled: 0 }
      for (const b of data) {
        const s = (b.status || b.bookingStatus || '').toLowerCase()
        if (s === 'pending') counts.pending++
        else if (s === 'confirmed') counts.confirmed++
        else if (s === 'in_progress' || s === 'inprogress') counts.inProgress++
        else if (s === 'completed') counts.completed++
        else if (s === 'cancelled') counts.cancelled++
      }
      bookings.value = counts
    }

    if (containerRes.status === 'fulfilled') {
      const data = containerRes.value.data as any[]
      const counts = { total: data.length, available: 0, allocated: 0, inTransit: 0 }
      for (const c of data) {
        const s = (c.status || '').toLowerCase()
        if (s === 'available') counts.available++
        else if (s === 'allocated') counts.allocated++
        else if (s === 'in_transit' || s === 'intransit') counts.inTransit++
      }
      containers.value = counts
    }

    if (notifRes.status === 'fulfilled') {
      const data = notifRes.value.data as any[]
      notifications.value = { total: data.length, unread: 0 }
      recentNotifications.value = data.slice(-5).reverse()
    }
  } catch (e: any) {
    error.value = 'Failed to load dashboard data. Make sure the services are running.'
  } finally {
    loading.value = false
  }
}

onMounted(fetchDashboard)
</script>

<template>
  <div class="p-6">
    <h2 class="text-2xl font-bold text-gray-800 mb-6">Dashboard</h2>

    <div v-if="loading" class="flex items-center justify-center h-64">
      <div class="animate-spin w-8 h-8 border-4 border-primary-500 border-t-transparent rounded-full"></div>
    </div>

    <div v-else-if="error" class="bg-yellow-50 border border-yellow-200 text-yellow-800 rounded-lg p-4 text-sm">
      {{ error }}
    </div>

    <template v-else>
      <!-- Top-line metrics -->
      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 mb-8">
        <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-4">
          <p class="text-xs text-gray-500 uppercase tracking-wider">Active Shipments</p>
          <p class="text-3xl font-bold text-gray-800 mt-1">{{ bookings.inProgress }}</p>
          <p class="text-xs text-gray-400 mt-1">{{ bookings.total }} total bookings</p>
        </div>
        <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-4">
          <p class="text-xs text-gray-500 uppercase tracking-wider">Total Containers</p>
          <p class="text-3xl font-bold text-gray-800 mt-1">{{ containers.total }}</p>
          <p class="text-xs text-green-600 mt-1">{{ containers.available }} available</p>
        </div>
        <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-4">
          <p class="text-xs text-gray-500 uppercase tracking-wider">Container Utilization</p>
          <p class="text-3xl font-bold text-gray-800 mt-1">{{ Math.round(utilization.utilizationPercent) }}%</p>
          <p class="text-xs text-gray-400 mt-1">{{ utilization.allocatedTeu }} / {{ utilization.totalCapacityTeu }} TEU</p>
        </div>
        <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-4">
          <p class="text-xs text-gray-500 uppercase tracking-wider">Pending Bookings</p>
          <p class="text-3xl font-bold text-gray-800 mt-1">{{ bookings.pending }}</p>
          <p class="text-xs text-gray-400 mt-1">Awaiting confirmation</p>
        </div>
      </div>

      <!-- Charts row -->
      <div class="grid grid-cols-1 lg:grid-cols-2 gap-6 mb-8">
        <!-- Booking status distribution -->
        <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-4">
          <h3 class="text-sm font-semibold text-gray-700 mb-3">Booking Status Distribution</h3>
          <div class="h-64 flex items-center justify-center text-gray-400 text-sm">
            <Doughnut
              :data="{
                labels: ['Pending', 'Confirmed', 'In Progress', 'Completed', 'Cancelled'],
                datasets: [{
                  data: [bookings.pending, bookings.confirmed, bookings.inProgress, bookings.completed, bookings.cancelled],
                  backgroundColor: ['#f59e0b', '#3b82f6', '#8b5cf6', '#10b981', '#ef4444']
                }]
              }"
              :options="chartOptions"
              v-if="bookings.total > 0"
            />
            <span v-else>No booking data</span>
          </div>
        </div>

        <!-- Container utilization chart -->
        <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-4">
          <h3 class="text-sm font-semibold text-gray-700 mb-3">Container Status Overview</h3>
          <div class="h-64 flex items-center justify-center text-gray-400 text-sm">
            <Doughnut
              :data="{
                labels: ['Available', 'Allocated', 'In Transit', 'Other'],
                datasets: [{
                  data: [containers.available, containers.allocated, containers.inTransit, containers.total - containers.available - containers.allocated - containers.inTransit],
                  backgroundColor: ['#10b981', '#3b82f6', '#8b5cf6', '#d1d5db']
                }]
              }"
              :options="chartOptions"
              v-if="containers.total > 0"
            />
            <span v-else>No container data</span>
          </div>
        </div>
      </div>

      <!-- Recent events feed -->
      <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-4">
        <h3 class="text-sm font-semibold text-gray-700 mb-3">Recent Operational Events</h3>
        <div v-if="recentNotifications.length === 0" class="text-gray-400 text-sm text-center py-4">
          No recent notifications
        </div>
        <div v-else class="space-y-2">
          <div v-for="n in recentNotifications" :key="n.id" class="flex items-start gap-3 py-2 border-b border-gray-50 last:border-0">
            <span class="text-lg">🔔</span>
            <div>
              <p class="text-sm text-gray-700">{{ n.title || n.message }}</p>
              <p class="text-xs text-gray-400">{{ new Date(n.createdAt || n.timestamp).toLocaleString() }}</p>
            </div>
          </div>
        </div>
      </div>
    </template>
  </div>
</template>