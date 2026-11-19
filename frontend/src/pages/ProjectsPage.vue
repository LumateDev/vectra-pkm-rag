<template>
  <section aria-labelledby="projects-title">
    <h1 id="projects-title">Проекты</h1>
    <p>Вы вошли как {{ authStore.currentUser?.userName }}.</p>
    <q-btn label="Выйти" outline no-caps :loading="authStore.isLoggingOut" @click="onLogout" />
  </section>
</template>

<script setup lang="ts">
import { useMeta } from 'quasar';
import { getApiErrorMessage } from '../api/axios-instance';
import { useNotify } from '../composables/useNotify';
import { useAuthStore } from '../stores/auth';

const authStore = useAuthStore();
const { notifyError } = useNotify();
useMeta({ title: 'Проекты · VectraPKM' });

async function onLogout() {
  try {
    await authStore.logout();
  } catch (error) {
    notifyError(getApiErrorMessage(error));
  }
}
</script>

<style scoped lang="scss">
h1 {
  font-size: 1.75rem;
  line-height: 1.25;
}
</style>
