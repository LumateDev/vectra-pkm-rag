<template>
  <section class="projects" aria-labelledby="projects-title">
    <header class="projects-header">
      <h1 id="projects-title">Проекты</h1>
      <q-btn label="Выйти" flat no-caps :loading="authStore.isLoggingOut" @click="onLogout" />
    </header>

    <q-form class="project-form" @submit="createProject">
      <q-input
        v-model="projectName"
        outlined
        label="Название проекта"
        :disable="isCreating"
        @update:model-value="createError = ''"
      />
      <q-btn
        type="submit"
        label="Создать проект"
        color="primary"
        unelevated
        no-caps
        :loading="isCreating"
        :disable="isLoading || !projectName.trim()"
      />
    </q-form>
    <p v-if="createError" class="projects-error" role="alert">{{ createError }}</p>

    <div class="projects-scroll" role="region" aria-label="Список проектов" tabindex="0">
      <p v-if="isLoading" role="status">Загружаем проекты…</p>
      <template v-else>
        <div v-if="listError" class="projects-list-error">
          <p class="projects-error" role="alert">{{ listError }}</p>
          <q-btn label="Повторить" flat no-caps :disable="isCreating" @click="loadProjects" />
        </div>
        <ul v-if="projects.length" class="project-grid">
          <li v-for="project in projects" :key="project.id" class="project-card">
            <div class="project-icon" aria-hidden="true">
              <q-icon name="folder_open" size="1.375rem" />
            </div>
            <h2>{{ project.name }}</h2>
            <p class="project-date">
              Создан
              <time :datetime="project.createdAt">{{ formatProjectDate(project.createdAt) }}</time>
            </p>
          </li>
        </ul>
        <p v-else-if="!listError" class="projects-empty">У вас пока нет проектов.</p>
      </template>
    </div>
  </section>
</template>

<script setup lang="ts">
import { useMeta } from 'quasar';
import { onMounted, ref } from 'vue';
import { getApiErrorMessage } from '../api/axios-instance';
import { getProjects } from '../api/generated/endpoints/projects/projects';
import type { ProjectResponse } from '../api/generated/models';
import { useNotify } from '../composables/useNotify';
import { useAuthStore } from '../stores/auth';

const authStore = useAuthStore();
const api = getProjects();
const projectDate = new Intl.DateTimeFormat('ru-RU', { dateStyle: 'long' });
const { notifyError } = useNotify();
useMeta({ title: 'Проекты · VectraPKM' });

const projects = ref<ProjectResponse[]>([]);
const projectName = ref('');
const isLoading = ref(true);
const isCreating = ref(false);
const listError = ref('');
const createError = ref('');

onMounted(loadProjects);

function formatProjectDate(value: string) {
  return projectDate.format(new Date(value));
}

async function loadProjects() {
  isLoading.value = true;
  listError.value = '';
  try {
    const response = await api.listProjects();
    if (response.error || !response.data) {
      throw new Error(response.error || 'Не удалось загрузить список проектов.');
    }
    projects.value = response.data;
  } catch (error) {
    listError.value = getApiErrorMessage(error);
  } finally {
    isLoading.value = false;
  }
}

async function createProject() {
  const name = projectName.value.trim();
  if (!name || isLoading.value || isCreating.value) return;

  isCreating.value = true;
  createError.value = '';
  try {
    const response = await api.createProject({ name });
    if (response.error || !response.data) {
      throw new Error(response.error || 'Не удалось создать проект.');
    }
    projects.value.unshift(response.data);
    projectName.value = '';
  } catch (error) {
    createError.value = getApiErrorMessage(error);
  } finally {
    isCreating.value = false;
  }
}

async function onLogout() {
  try {
    await authStore.logout();
  } catch (error) {
    notifyError(getApiErrorMessage(error));
  }
}
</script>

<style scoped lang="scss">
@use '../css/theme/contract' as theme;

.projects {
  display: flex;
  flex-direction: column;
  gap: clamp(0.75rem, 2.5dvh, 1.5rem);
  width: min(100%, 66rem);
  min-width: 0;
  min-height: 0;
}

.projects-header,
.projects-list-error {
  display: flex;
  align-items: center;
  justify-content: space-between;
  flex-wrap: wrap;
  gap: 0.75rem;
}

.project-form {
  display: grid;
  grid-template-columns: minmax(0, 1fr) auto;
  gap: 0.75rem;
  padding: 1rem;
  border: 1px solid theme.token('app-border');
  border-radius: 1rem;
  background: theme.token('app-surface');
  box-shadow: theme.token('app-card-shadow');
}

.projects-scroll {
  flex: 1;
  min-height: 0;
  overflow: hidden auto;
  overscroll-behavior-y: contain;
  scrollbar-gutter: stable;
  scrollbar-width: thin;
  scrollbar-color: color-mix(in srgb, theme.token('app-muted') 55%, transparent) transparent;
  padding: 0.375rem 0.25rem 0.75rem;

  &:focus-visible {
    outline: 2px solid theme.token('app-link');
    outline-offset: -2px;
    border-radius: 0.75rem;
  }
}

.project-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(min(100%, 17rem), 1fr));
  gap: 1rem;
  margin: 0;
  padding: 0;
  list-style: none;
}

.project-card {
  display: grid;
  align-content: start;
  gap: 0.875rem;
  min-width: 0;
  padding: 1.25rem;
  border: 1px solid theme.token('app-border');
  border-radius: 1rem;
  background: linear-gradient(
    145deg,
    color-mix(in srgb, theme.token('app-primary') 5%, theme.token('app-surface')),
    theme.token('app-surface') 65%
  );
  box-shadow: theme.token('app-card-shadow');
  overflow-wrap: anywhere;
}

.project-icon {
  display: grid;
  place-items: center;
  width: 2.75rem;
  height: 2.75rem;
  border-radius: 0.75rem;
  background: theme.token('app-notice');
  color: theme.token('app-link');
}

h1 {
  font-size: clamp(1.875rem, 5vw, 2.5rem);
  line-height: 1.15;
  font-weight: 650;
}

h2 {
  font-size: 1.125rem;
  line-height: 1.35;
  font-weight: 650;
}

h1,
h2,
p {
  margin: 0;
}

.project-date,
.projects-empty {
  color: theme.token('app-muted');
}

.project-date {
  font-size: 0.875rem;
}

.projects-error {
  color: theme.token('app-negative');
}

@media (hover: hover) and (pointer: fine) {
  .project-card:hover {
    border-color: color-mix(in srgb, theme.token('app-link') 45%, theme.token('app-border'));
    box-shadow: 0 12px 28px color-mix(in srgb, theme.token('app-text') 12%, transparent);
  }
}

@media (hover: hover) and (pointer: fine) and (prefers-reduced-motion: no-preference) {
  .project-card {
    transition:
      translate 180ms ease,
      border-color 180ms ease,
      box-shadow 180ms ease;

    &:hover {
      translate: 0 -3px;
    }
  }
}

@media (max-width: 34rem) {
  .project-form {
    grid-template-columns: 1fr;
  }
}
</style>
