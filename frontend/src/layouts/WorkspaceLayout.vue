<script setup lang="ts">
import { onMounted, ref, watch } from 'vue';
import { useRoute } from 'vue-router';
import AppBrand from '../components/AppBrand.vue';
import AppSidebar from '../components/AppSidebar.vue';
import { provideWorkspace } from '../composables/useWorkspace';

const { loadProjects } = provideWorkspace();
const route = useRoute();
const sidebarOpen = ref(false);
const sidebarInLayout = ref(false);
const main = ref<HTMLElement>();

onMounted(loadProjects);
watch(
  () => route.path,
  () => main.value?.scrollTo(0, 0),
  { flush: 'post' },
);
</script>

<template>
  <q-layout container view="hHh Lpr fff" class="workspace">
    <q-header class="workspace-header">
      <q-toolbar class="workspace-toolbar">
        <q-btn
          v-if="!sidebarInLayout"
          flat
          round
          icon="menu"
          size="0.875rem"
          aria-controls="workspace-sidebar"
          :aria-expanded="sidebarOpen"
          :aria-label="sidebarOpen ? 'Закрыть боковую панель' : 'Открыть боковую панель'"
          @click="sidebarOpen = !sidebarOpen"
        />
        <router-link
          class="workspace-brand"
          :to="{ name: 'projects' }"
          aria-label="Vectra PKM — проекты"
        >
          <AppBrand />
        </router-link>
      </q-toolbar>
    </q-header>

    <q-drawer
      id="workspace-sidebar"
      v-model="sidebarOpen"
      aria-label="Боковая панель"
      show-if-above
      :width="248"
      :breakpoint="760"
      @on-layout="sidebarInLayout = $event"
    >
      <AppSidebar :overlay="!sidebarInLayout" @close="sidebarOpen = false" />
    </q-drawer>

    <q-page-container class="workspace-container">
      <main ref="main" class="workspace-main">
        <router-view />
      </main>
    </q-page-container>
  </q-layout>
</template>

<style scoped lang="scss">
@use '../css/theme/contract' as theme;

.workspace {
  height: 100dvh;
  background: theme.token('app-surface');
}

.workspace-header {
  border-bottom: 1px solid theme.token('app-border');
  background: theme.token('app-surface');
  color: theme.token('app-text');
}

.workspace-toolbar {
  min-height: 3.25rem;
  gap: 0.5rem;
  padding: 0.375rem 1rem;
}

.workspace-brand {
  display: inline-flex;
  border-radius: 0.375rem;
  font-size: 1.0625rem;
  text-decoration: none;
}

.workspace-container {
  height: 100%;
}

.workspace-main {
  display: grid;
  grid-template-rows: minmax(0, 1fr);
  justify-items: center;
  height: 100%;
  min-width: 0;
  overflow: auto;
  padding: clamp(0.75rem, 3vw, 2rem);
}
</style>
