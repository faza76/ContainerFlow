<script setup lang="ts">
import { ref } from 'vue'
import { useAuthStore } from '@/stores/auth'
import api from '@/api/client'
import { useRouter } from 'vue-router'

const auth = useAuthStore()
const router = useRouter()
const username = ref('')
const error = ref('')
const loading = ref(false)

async function handleLogin() {
  if (!username.value.trim()) {
    error.value = 'Please enter a username'
    return
  }
  error.value = ''
  loading.value = true
  try {
    await auth.login(username.value.trim())
    router.push('/dashboard')
  } catch (e: any) {
    error.value = e.response?.data?.error || 'Login failed. Try admin, staff, or customer.'
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="min-h-screen flex items-center justify-center bg-gradient-to-br from-primary-500 to-blue-800">
    <div class="bg-white rounded-2xl shadow-xl p-8 w-full max-w-sm">
      <div class="text-center mb-6">
        <h1 class="text-2xl font-bold text-gray-800">ContainerFlow</h1>
        <p class="text-sm text-gray-500 mt-1">Container Tracking Dashboard</p>
      </div>

      <form @submit.prevent="handleLogin" class="space-y-4">
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-1">Username</label>
          <input
            v-model="username"
            type="text"
            placeholder="admin, staff, or customer"
            class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent outline-none"
            :disabled="loading"
          />
        </div>

        <p v-if="error" class="text-red-600 text-sm">{{ error }}</p>

        <button
          type="submit"
          :disabled="loading"
          class="w-full bg-primary-600 text-white py-2 rounded-lg font-medium hover:bg-primary-700 disabled:opacity-50"
        >
          {{ loading ? 'Signing in...' : 'Sign in' }}
        </button>
      </form>

      <div class="mt-4 text-xs text-gray-400 text-center">
        <p>Demo accounts: <strong>admin</strong>, <strong>staff</strong>, <strong>customer</strong></p>
        <p class="mt-1">No password required (dev mode)</p>
      </div>
    </div>
  </div>
</template>