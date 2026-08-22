<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import api from '@/api/client'

const route = useRoute()
const router = useRouter()
const auth = useAuthStore()
const id = route.params.id as string

const booking = ref<any>(null)
const loading = ref(true)
const error = ref('')
const actionLoading = ref(false)
const actionError = ref('')

const canManage = computed(() => auth.canManageBookings)

const statusSteps = ['PENDING', 'CONFIRMED', 'IN_PROGRESS', 'COMPLETED']
const currentStepIndex = computed(() => {
  const s = (booking.value?.status || booking.value?.bookingStatus || '').toUpperCase()
  const idx = statusSteps.indexOf(s)
  return idx >= 0 ? idx : -1
})

async function fetchBooking() {
  try {
    const res = await api.get(`/api/bookings/${id}`)
    booking.value = res.data
  } catch (e: any) {
    error.value = 'Booking not found'
  } finally {
    loading.value = false
  }
}

async function confirmBooking() {
  actionLoading.value = true
  actionError.value = ''
  try {
    await api.post(`/api/bookings/${id}/confirm`)
    await fetchBooking()
  } catch (e: any) {
    actionError.value = e.response?.data?.error || 'Failed to confirm booking'
  } finally {
    actionLoading.value = false
  }
}

async function cancelBooking() {
  actionLoading.value = true
  actionError.value = ''
  try {
    await api.post(`/api/bookings/${id}/cancel`)
    await fetchBooking()
  } catch (e: any) {
    actionError.value = e.response?.data?.error || 'Failed to cancel booking'
  } finally {
    actionLoading.value = false
  }
}

onMounted(fetchBooking)
</script>

<template>
  <div class="p-6">
    <button @click="router.push('/bookings')" class="text-sm text-primary-600 hover:text-primary-800 mb-4">← Back to Bookings</button>

    <div v-if="loading" class="flex items-center justify-center h-64">
      <div class="animate-spin w-8 h-8 border-4 border-primary-500 border-t-transparent rounded-full"></div>
    </div>

    <div v-else-if="error" class="text-center py-16">
      <p class="text-red-500">{{ error }}</p>
    </div>

    <template v-else-if="booking">
      <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-6 mb-6">
        <div class="flex items-start justify-between mb-4">
          <div>
            <h2 class="text-2xl font-bold text-gray-800">Booking {{ booking.bookingNumber || id.slice(0, 8) }}</h2>
            <p class="text-sm text-gray-500 mt-1">Customer: {{ booking.customerName || booking.customerId }}</p>
          </div>
          <span class="px-3 py-1 rounded-full text-sm font-medium"
            :class="{
              'bg-yellow-100 text-yellow-800': (booking.status || booking.bookingStatus) === 'Pending',
              'bg-blue-100 text-blue-800': (booking.status || booking.bookingStatus) === 'Confirmed',
              'bg-purple-100 text-purple-800': (booking.status || booking.bookingStatus) === 'InProgress',
              'bg-green-100 text-green-800': (booking.status || booking.bookingStatus) === 'Completed',
              'bg-red-100 text-red-800': (booking.status || booking.bookingStatus) === 'Cancelled'
            }"
          >
            {{ booking.status || booking.bookingStatus }}
          </span>
        </div>

        <!-- Status progression -->
        <div class="my-6">
          <div class="flex items-center justify-between">
            <div v-for="(step, i) in statusSteps" :key="step"
              class="flex items-center"
              :class="{ 'flex-1': i < statusSteps.length - 1 }">
              <div class="flex items-center justify-center w-8 h-8 rounded-full text-sm font-bold"
                :class="{
                  'bg-primary-600 text-white': i <= currentStepIndex && currentStepIndex >= 0,
                  'bg-gray-200 text-gray-400': i > currentStepIndex || currentStepIndex < 0
                }">
                {{ i + 1 }}
              </div>
              <div v-if="i < statusSteps.length - 1" class="flex-1 h-1 mx-2"
                :class="i < currentStepIndex ? 'bg-primary-600' : 'bg-gray-200'"></div>
            </div>
          </div>
          <div class="flex justify-between mt-2 text-xs text-gray-500">
            <span v-for="step in statusSteps" :key="step">{{ step }}</span>
          </div>
        </div>

        <div class="grid grid-cols-2 gap-4 text-sm mt-6">
          <div><span class="text-gray-500">Container Type:</span> {{ booking.containerType || '—' }}</div>
          <div><span class="text-gray-500">Container Count:</span> {{ booking.containerCount || 1 }}</div>
          <div><span class="text-gray-500">Created:</span> {{ booking.createdAt ? new Date(booking.createdAt).toLocaleString() : '—' }}</div>
          <div><span class="text-gray-500">Last Updated:</span> {{ booking.updatedAt ? new Date(booking.updatedAt).toLocaleString() : '—' }}</div>
        </div>

        <!-- Actions -->
        <div v-if="canManage" class="mt-6 flex gap-3">
          <button
            v-if="(booking.status || booking.bookingStatus || '').toLowerCase() === 'pending'"
            @click="confirmBooking" :disabled="actionLoading"
            class="px-4 py-2 bg-primary-600 text-white rounded-lg text-sm font-medium hover:bg-primary-700 disabled:opacity-50">
            {{ actionLoading ? 'Processing...' : 'Confirm Booking' }}
          </button>
          <button
            v-if="['pending', 'confirmed', 'in_progress', 'inprogress'].includes((booking.status || booking.bookingStatus || '').toLowerCase())"
            @click="cancelBooking" :disabled="actionLoading"
            class="px-4 py-2 bg-red-600 text-white rounded-lg text-sm font-medium hover:bg-red-700 disabled:opacity-50">
            {{ actionLoading ? 'Processing...' : 'Cancel Booking' }}
          </button>
        </div>
        <p v-if="actionError" class="mt-2 text-red-600 text-sm">{{ actionError }}</p>
      </div>
    </template>
  </div>
</template>