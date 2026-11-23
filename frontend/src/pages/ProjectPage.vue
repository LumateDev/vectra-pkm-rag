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
    <header class="project-header">
      <h1 id="project-title">{{ project?.name ?? 'Проект' }}</h1>
      <q-btn
        v-if="project && !isEditing"
        label="Переименовать"
        icon="edit"
        flat
        no-caps
        @click="startRename"
      />
    </header>
    <p v-if="isLoading" role="status">Загружаем проект…</p>
    <p v-else-if="error" class="project-error" role="alert">{{ error }}</p>

    <q-form v-if="isEditing" class="rename-form" @submit="renameProject">
      <q-input
        v-model="newName"
        outlined
        autofocus
        label="Название проекта"
        :disable="isSaving"
        @update:model-value="renameError = ''"
      />
      <div class="rename-actions">
        <q-btn label="Отмена" flat no-caps :disable="isSaving" @click="isEditing = false" />
        <q-btn
          type="submit"
          label="Сохранить"
          color="primary"
          unelevated
          no-caps
          :loading="isSaving"
          :disable="!newName.trim() || newName.trim() === project?.name"
        />
      </div>
      <p v-if="renameError" class="project-error" role="alert">{{ renameError }}</p>
    </q-form>

    <project-notes v-if="project" :key="project.id" :project-id="project.id" />
  </section>
</template>

<script setup lang="ts">
import { isAxiosError } from 'axios';
import { useMeta } from 'quasar';
import { ref, watch } from 'vue';
import { getApiErrorMessage } from '../api/axios-instance';
import { getProjects } from '../api/generated/endpoints/projects/projects';
import type { ProjectResponse } from '../api/generated/models';
import ProjectNotes from '../components/ProjectNotes.vue';
import { useWorkspace } from '../composables/useWorkspace';

const props = defineProps<{ id: string }>();
const api = getProjects();
const { saveProject } = useWorkspace();
const project = ref<ProjectResponse>();
const isLoading = ref(true);
const error = ref('');
const isEditing = ref(false);
const isSaving = ref(false);
const newName = ref('');
const renameError = ref('');
let projectRequest: AbortController | undefined;

useMeta(() => ({ title: `${project.value?.name ?? 'Проект'} · VectraPKM` }));

watch(
  () => props.id,
  async (id, _, onCleanup) => {
    const request = new AbortController();
    projectRequest = request;
    onCleanup(() => request.abort());
    project.value = undefined;
    isLoading.value = true;
    error.value = '';
    isEditing.value = false;
    isSaving.value = false;

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

function startRename() {
  if (!project.value) return;
  newName.value = project.value.name;
  renameError.value = '';
  isEditing.value = true;
}

async function renameProject() {
  const currentProject = project.value;
  const name = newName.value.trim();
  const signal = projectRequest?.signal;
  if (!currentProject || !signal || !name || name === currentProject.name || isSaving.value) {
    return;
  }

  isSaving.value = true;
  renameError.value = '';
  try {
    const response = await api.renameProject(currentProject.id, { name }, { signal });
    if (signal.aborted) return;
    if (response.error || !response.data) {
      throw new Error(response.error || 'Не удалось переименовать проект.');
    }

    project.value = response.data;
    saveProject(response.data);
    isEditing.value = false;
  } catch (cause) {
    if (!signal.aborted) renameError.value = getApiErrorMessage(cause);
  } finally {
    if (!signal.aborted) isSaving.value = false;
  }
}
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

.project-header,
.rename-actions {
  display: flex;
  align-items: center;
  flex-wrap: wrap;
  gap: 0.75rem;
}

.project-header {
  justify-content: space-between;
}

.rename-form {
  display: grid;
  gap: 0.75rem;
  width: min(100%, 33rem);
  margin-top: 1.25rem;
  padding: 1rem;
  border: 1px solid theme.token('app-border');
  border-radius: 1rem;
  background: theme.token('app-surface');
  box-shadow: theme.token('app-card-shadow');
}

.rename-actions {
  justify-content: flex-end;
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

.rename-form .project-error {
  margin: 0;
}
</style>
