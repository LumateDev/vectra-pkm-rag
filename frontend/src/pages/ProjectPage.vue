<template>
  <section class="project" aria-labelledby="project-title">
    <q-btn
      :to="{ name: 'projects' }"
      icon="arrow_back"
      label="Все проекты"
      flat
      no-caps
      class="back-link"
    />
    <h1 id="project-title">{{ project?.name ?? 'Проект' }}</h1>
    <p v-if="isLoading" role="status">Загружаем проект…</p>
    <p v-else-if="error" class="project-error" role="alert">{{ error }}</p>
  </section>
</template>

<script setup lang="ts">
import { isAxiosError } from 'axios';
import { useMeta } from 'quasar';
import { ref, watch } from 'vue';
import { getApiErrorMessage } from '../api/axios-instance';
import { getProjects } from '../api/generated/endpoints/projects/projects';
import type { ProjectResponse } from '../api/generated/models';

const props = defineProps<{ id: string }>();
const api = getProjects();
const project = ref<ProjectResponse>();
const isLoading = ref(true);
const error = ref('');

useMeta(() => ({ title: `${project.value?.name ?? 'Проект'} · VectraPKM` }));

watch(
  () => props.id,
  async (id, _, onCleanup) => {
    const request = new AbortController();
    onCleanup(() => request.abort());
    project.value = undefined;
    isLoading.value = true;
    error.value = '';

    try {
      const response = await api.getProject(id, { signal: request.signal });
      if (request.signal.aborted) return;
      if (response.error || !response.data) {
        throw new Error(response.error || 'Не удалось загрузить проект.');
      }
      project.value = response.data;
    } catch (cause) {
      if (request.signal.aborted) return;
      error.value =
        isAxiosError(cause) && cause.response?.status === 404
          ? 'Проект не найден.'
          : getApiErrorMessage(cause);
    } finally {
      if (!request.signal.aborted) isLoading.value = false;
    }
  },
  { immediate: true },
);
</script>

<style scoped lang="scss">
@use '../css/theme/contract' as theme;

.project {
  width: min(100%, 66rem);
  min-width: 0;
  overflow: auto;
  overflow-wrap: anywhere;
}

.back-link {
  margin-bottom: 1rem;
  color: theme.token('app-link');
}

h1 {
  margin: 0;
  font-size: clamp(1.875rem, 5vw, 2.5rem);
  line-height: 1.15;
  font-weight: 650;
}

.project-error {
  color: theme.token('app-negative');
}
</style>
