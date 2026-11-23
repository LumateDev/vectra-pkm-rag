<script setup lang="ts">
import { computed, reactive, watch } from 'vue';
import { useRoute } from 'vue-router';
import { useWorkspace } from '../composables/useWorkspace';
import WorkspaceAccount from './WorkspaceAccount.vue';

defineProps<{ overlay: boolean }>();
const emit = defineEmits<{ close: [] }>();
const route = useRoute();
const { projects, notesByProject, loadProjects, loadNotes } = useWorkspace();
const activeProjectId = computed(() =>
  typeof route.params.id === 'string' ? route.params.id : undefined,
);
const expanded = reactive(new Set<string>());

watch(
  activeProjectId,
  (id) => {
    if (id) {
      expanded.add(id);
      void loadNotes(id);
    }
  },
  { immediate: true },
);

function toggleProject(id: string) {
  if (expanded.delete(id)) return;
  expanded.add(id);
  void loadNotes(id);
}
</script>

<template>
  <div class="sidebar">
    <nav class="sidebar-nav" aria-label="Проекты и заметки">
      <div class="sidebar-heading">
        <router-link class="sidebar-home" :to="{ name: 'projects' }">
          <q-icon name="account_tree" size="1.125rem" aria-hidden="true" />
          <span>Проекты</span>
        </router-link>
        <q-btn
          v-if="overlay"
          flat
          round
          icon="close"
          size="0.875rem"
          aria-label="Закрыть боковую панель"
          @click="emit('close')"
        />
      </div>

      <p v-if="projects.status === 'loading'" class="sidebar-message" role="status">
        Загружаем проекты…
      </p>
      <div v-else-if="projects.status === 'error'" class="sidebar-message">
        <p class="sidebar-error" role="alert">{{ projects.error }}</p>
        <q-btn label="Повторить" flat no-caps size="0.8125rem" @click="loadProjects" />
      </div>
      <p v-else-if="!projects.items.length" class="sidebar-message">У вас пока нет проектов.</p>

      <ul class="sidebar-list">
        <li v-for="project in projects.items" :key="project.id">
          <div class="project-row" :class="{ 'current-project': project.id === activeProjectId }">
            <q-btn
              class="branch-toggle"
              flat
              dense
              :icon="expanded.has(project.id) ? 'expand_more' : 'chevron_right'"
              size="0.875rem"
              :aria-expanded="expanded.has(project.id)"
              :aria-controls="`sidebar-notes-${project.id}`"
              :aria-label="`${expanded.has(project.id) ? 'Свернуть' : 'Развернуть'} проект «${project.name}»`"
              @click="toggleProject(project.id)"
            />
            <router-link
              class="project-link"
              :to="{ name: 'project', params: { id: project.id } }"
              :title="project.name"
            >
              <q-icon
                :name="expanded.has(project.id) ? 'folder_open' : 'folder'"
                size="1rem"
                aria-hidden="true"
              />
              <span class="row-label">{{ project.name }}</span>
            </router-link>
          </div>

          <div
            v-show="expanded.has(project.id)"
            :id="`sidebar-notes-${project.id}`"
            class="note-branch"
          >
            <p
              v-if="notesByProject.get(project.id)?.status === 'loading'"
              class="sidebar-message"
              role="status"
            >
              Загружаем заметки…
            </p>
            <div
              v-else-if="notesByProject.get(project.id)?.status === 'error'"
              class="sidebar-message"
            >
              <p class="sidebar-error" role="alert">{{ notesByProject.get(project.id)?.error }}</p>
              <q-btn
                label="Повторить"
                flat
                no-caps
                size="0.8125rem"
                @click="loadNotes(project.id)"
              />
            </div>
            <ul v-else-if="notesByProject.get(project.id)?.items.length" class="sidebar-list">
              <li v-for="note in notesByProject.get(project.id)?.items" :key="note.id">
                <router-link
                  class="note-link"
                  :to="{ name: 'note', params: { id: project.id, noteId: note.id } }"
                  :title="note.title"
                >
                  <q-icon name="description" size="1rem" aria-hidden="true" />
                  <span class="row-label">{{ note.title }}</span>
                </router-link>
              </li>
            </ul>
            <p v-else class="sidebar-message">Пока нет заметок.</p>
          </div>
        </li>
      </ul>
    </nav>
    <WorkspaceAccount />
  </div>
</template>

<style scoped lang="scss">
@use '../css/theme/contract' as theme;

.sidebar {
  display: flex;
  flex-direction: column;
  height: 100%;
  background: theme.token('app-background');
  border-inline-end: 1px solid theme.token('app-border');
  color: theme.token('app-text');
}

.sidebar-nav {
  flex: 1;
  min-height: 0;
  padding: 0.625rem;
  overflow: auto;
  overscroll-behavior: contain;
  scrollbar-width: thin;
}

.sidebar-home,
.project-link,
.note-link {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  min-height: 2.125rem;
  padding: 0.3rem 0.5rem;
  border-radius: 0.375rem;
  color: inherit;
  text-decoration: none;

  &:hover {
    background: theme.token('app-surface-soft');
  }

  &:focus-visible {
    outline-offset: -2px;
  }

  &[aria-current='page'] {
    background: theme.token('app-notice');
    color: theme.token('app-link');
    font-weight: 650;
  }
}

.sidebar-heading {
  display: flex;
  align-items: center;
  gap: 0.25rem;
  margin-bottom: 0.375rem;
}

.sidebar-home {
  flex: 1;
  min-height: 2.5rem;
  font-weight: 650;
}

.sidebar-list {
  display: grid;
  gap: 0.125rem;
  margin: 0;
  padding: 0;
  list-style: none;
}

.project-row {
  display: flex;
  align-items: stretch;
  border-radius: 0.375rem;
}

.current-project .project-link {
  color: theme.token('app-link');
  font-weight: 650;
}

.branch-toggle {
  flex: 0 0 1.875rem;
  border-radius: 0.375rem;
  color: theme.token('app-muted');
}

.project-link {
  flex: 1;
  min-width: 0;
  padding-inline-start: 0.25rem;
}

.note-branch {
  margin: 0.125rem 0 0.25rem 0.875rem;
  padding-inline-start: 0.5rem;
  border-inline-start: 1px solid theme.token('app-border');
}

.row-label {
  min-width: 0;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.sidebar-message {
  margin: 0;
  padding: 0.375rem 0.5rem;
  color: theme.token('app-muted');
  font-size: 0.8125rem;
  overflow-wrap: anywhere;
}

.sidebar-error {
  margin: 0;
  color: theme.token('app-negative');
}

@media (pointer: coarse) {
  .sidebar-home,
  .project-link,
  .note-link {
    min-height: 2.75rem;
  }

  .branch-toggle {
    flex-basis: 2.75rem;
  }
}
</style>
