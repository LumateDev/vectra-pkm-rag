<template>
  <section class="note-page" aria-labelledby="note-title">
    <q-btn
      :to="{ name: 'project', params: { id } }"
      icon="arrow_back"
      label="К проекту"
      flat
      no-caps
      class="back-link"
    />
    <h1 id="note-title">{{ heading }}</h1>
    <p v-if="isLoading" role="status">Загружаем заметку…</p>
    <p v-else-if="loadError" class="note-error" role="alert">{{ loadError }}</p>
    <q-form v-else class="note-form" @submit="saveNote">
      <q-input
        v-model="title"
        outlined
        label="Заголовок"
        :disable="isSaving"
        @update:model-value="saveError = ''"
      />
      <q-input
        v-model="markdown"
        outlined
        type="textarea"
        rows="12"
        label="Текст заметки"
        :disable="isSaving"
        @update:model-value="saveError = ''"
      />
      <p v-if="saveError" class="note-error" role="alert">{{ saveError }}</p>
      <q-btn
        class="save-button"
        type="submit"
        label="Сохранить"
        color="primary"
        unelevated
        no-caps
        :loading="isSaving"
        :disable="!title.trim()"
      />
    </q-form>
  </section>
</template>

<script setup lang="ts">
import { isAxiosError } from 'axios';
import { useMeta } from 'quasar';
import { computed, ref, watch } from 'vue';
import { useRouter } from 'vue-router';
import { getApiErrorMessage } from '../api/axios-instance';
import { getNotes } from '../api/generated/endpoints/notes/notes';
import { useNotify } from '../composables/useNotify';
import { useWorkspace } from '../composables/useWorkspace';

const props = defineProps<{ id: string; noteId?: string }>();
const router = useRouter();
const api = getNotes();
const { notifySuccess } = useNotify();
const { saveNote: updateWorkspaceNote } = useWorkspace();
const heading = computed(() => (props.noteId ? 'Редактирование заметки' : 'Новая заметка'));
const title = ref('');
const markdown = ref('');
const isLoading = ref(false);
const isSaving = ref(false);
const loadError = ref('');
const saveError = ref('');
let noteRequest: AbortController | undefined;

useMeta(() => ({ title: `${heading.value} · VectraPKM` }));

watch(
  () => [props.id, props.noteId] as const,
  async ([projectId, noteId], _, onCleanup) => {
    const request = new AbortController();
    noteRequest = request;
    onCleanup(() => request.abort());
    title.value = '';
    markdown.value = '';
    loadError.value = '';
    saveError.value = '';
    isSaving.value = false;
    isLoading.value = Boolean(noteId);
    if (!noteId) return;

    try {
      const result = await api.getNote(projectId, noteId, { signal: request.signal });
      if (request.signal.aborted) return;
      if (result.error || !result.data) {
        throw new Error(result.error || 'Не удалось открыть заметку.');
      }
      title.value = result.data.title;
      markdown.value = result.data.markdown;
    } catch (cause) {
      if (request.signal.aborted) return;
      loadError.value =
        isAxiosError(cause) && cause.response?.status === 404
          ? 'Заметка не найдена.'
          : getApiErrorMessage(cause);
    } finally {
      if (!request.signal.aborted) isLoading.value = false;
    }
  },
  { immediate: true },
);

async function saveNote() {
  const signal = noteRequest?.signal;
  const noteTitle = title.value.trim();
  if (!signal || !noteTitle || isLoading.value || isSaving.value) return;

  const { id, noteId } = props;
  const data = { title: noteTitle, markdown: markdown.value };
  isSaving.value = true;
  saveError.value = '';

  try {
    const result = noteId
      ? await api.updateNote(id, noteId, data, { signal })
      : await api.createNote(id, data, { signal });
    if (signal.aborted) return;
    if (result.error || !result.data) {
      throw new Error(result.error || 'Не удалось сохранить заметку.');
    }

    title.value = result.data.title;
    markdown.value = result.data.markdown;
    updateWorkspaceNote(id, result.data);
    notifySuccess('Заметка сохранена.');
    if (!noteId) await router.replace({ name: 'note', params: { id, noteId: result.data.id } });
  } catch (cause) {
    if (!signal.aborted) saveError.value = getApiErrorMessage(cause);
  } finally {
    if (!signal.aborted) isSaving.value = false;
  }
}
</script>

<style scoped lang="scss">
@use '../css/theme/contract' as theme;

.note-page {
  width: min(100%, 66rem);
  min-width: 0;
  overflow: auto;
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

.note-form {
  display: grid;
  gap: 1rem;
  margin-block: 1.25rem 1rem;
  padding: clamp(0.75rem, 2vw, 1.5rem);
  border: 1px solid theme.token('app-border');
  border-radius: 1rem;
  background: theme.token('app-surface');
  box-shadow: theme.token('app-card-shadow');
}

.save-button {
  justify-self: end;
}

.note-error {
  color: theme.token('app-negative');
}

.note-form .note-error {
  margin: 0;
}
</style>
