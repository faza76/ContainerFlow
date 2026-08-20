<script setup lang="ts">
import { useAuthStore } from '@/stores/auth'
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'

const auth = useAuthStore()
const router = useRouter()

onMounted(() => {
  auth.loadFromStorage()
  if (!auth.isAuthenticated && router.currentRoute.value.name !== 'login') {
    router.push('/login')
  }
})
</script>

<template>
  <router-view />
</template>