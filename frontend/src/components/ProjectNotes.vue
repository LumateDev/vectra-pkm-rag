<template>
  <section class="notes" aria-labelledby="notes-title">
    <header class="notes-header">
      <h2 id="notes-title">Заметки</h2>
      <q-btn
        :to="{ name: 'new-note', params: { id: projectId } }"
        icon="add"
        label="Новая заметка"
        color="primary"
        unelevated
        no-caps
      />
    </header>

    <p v-if="isLoading" role="status">Загружаем заметки…</p>
    <div v-else-if="error" class="notes-header">
      <p class="notes-error" role="alert">{{ error }}</p>
      <q-btn label="Повторить" flat no-caps @click="loadNotes" />
    </div>
    <ul v-else-if="notes.length" class="note-list">
      <li v-for="note in notes" :key="note.id">
        <router-link
          class="note-link"
          :to="{ name: 'note', params: { id: projectId, noteId: note.id } }"
        >
          <q-icon class="note-icon" name="description" size="1.25rem" aria-hidden="true" />
          <span class="note-title">{{ note.title }}</span>
          <time class="note-date" :datetime="note.updatedAt">
            {{ updatedDate.format(new Date(note.updatedAt)) }}
          </time>
        </router-link>
      </li>
    </ul>
    <p v-else class="notes-empty">В этом проекте пока нет заметок.</p>
  </section>
</template>

<script setup lang="ts">
import { computed, watch } from 'vue';
import { useWorkspace } from '../composables/useWorkspace';

const props = defineProps<{ projectId: string }>();
const workspace = useWorkspace();
const updatedDate = new Intl.DateTimeFormat('ru-RU', { dateStyle: 'medium' });
const list = computed(() => workspace.notesByProject.get(props.projectId));
const notes = computed(() => list.value?.items ?? []);
const isLoading = computed(() => !list.value || list.value.status === 'loading');
const error = computed(() => list.value?.error ?? '');

watch(
  () => props.projectId,
  () => {
    if (!list.value) void loadNotes();
  },
  { immediate: true },
);

function loadNotes() {
  return workspace.loadNotes(props.projectId);
}
</script>

<style scoped lang="scss">
@use '../css/theme/contract' as theme;

.notes {
  display: grid;
  gap: 1rem;
  margin-block: 2rem 1rem;
}

.notes-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  flex-wrap: wrap;
  gap: 0.75rem;
}

h2,
p {
  margin: 0;
}

h2 {
  font-size: 1.375rem;
  line-height: 1.25;
}

.note-list {
  margin: 0;
  padding: 0;
  border: 1px solid theme.token('app-border');
  border-radius: 1rem;
  background: theme.token('app-surface');
  box-shadow: theme.token('app-card-shadow');
  list-style: none;

  li + li {
    border-top: 1px solid theme.token('app-border');
  }
}

.note-link {
  display: grid;
  grid-template-columns: auto minmax(0, 1fr) auto;
  align-items: center;
  gap: 0.75rem;
  padding: 0.875rem 1rem;
  border-radius: 0.75rem;
  color: theme.token('app-text');
  text-decoration: none;

  &:hover {
    background: theme.token('app-surface-soft');
  }

  &:focus-visible {
    outline-offset: -2px;
  }
}

.note-icon {
  color: theme.token('app-link');
}

.note-title {
  font-weight: 600;
  overflow-wrap: anywhere;
}

.note-date,
.notes-empty {
  color: theme.token('app-muted');
}

.notes-error {
  color: theme.token('app-negative');
}

@media (max-width: 32rem) {
  .note-link {
    grid-template-columns: auto minmax(0, 1fr);
  }

  .note-date {
    grid-column: 2;
  }
}
</style>
