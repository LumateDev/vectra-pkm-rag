import { inject, onScopeDispose, provide, reactive, shallowReactive, type InjectionKey } from 'vue';
import { getApiErrorMessage } from '../api/axios-instance';
import { getNotes } from '../api/generated/endpoints/notes/notes';
import { getProjects } from '../api/generated/endpoints/projects/projects';
import type { NoteListItemResponse, ProjectResponse } from '../api/generated/models';

interface WorkspaceList<T> {
  items: T[];
  status: 'idle' | 'loading' | 'ready' | 'error';
  error: string;
}

function workspaceList<T>(): WorkspaceList<T> {
  return { items: [], status: 'idle', error: '' };
}

const workspaceKey: InjectionKey<ReturnType<typeof provideWorkspace>> = Symbol('workspace');

export function provideWorkspace() {
  const projects = reactive(workspaceList<ProjectResponse>());
  const notesByProject = shallowReactive(new Map<string, WorkspaceList<NoteListItemResponse>>());
  const requests = new Map<object, AbortController>();
  const projectsApi = getProjects();
  const notesApi = getNotes();
  let disposed = false;

  onScopeDispose(() => {
    disposed = true;
    requests.forEach((request) => request.abort());
    requests.clear();
  });

  async function load<T>(
    list: WorkspaceList<T>,
    requestItems: (signal: AbortSignal) => Promise<{ data?: T[] | null; error?: string | null }>,
    fallback: string,
  ) {
    if (disposed || requests.has(list)) return;
    const request = new AbortController();
    requests.set(list, request);
    if (list.status !== 'ready') list.status = 'loading';
    list.error = '';

    try {
      const response = await requestItems(request.signal);
      if (request.signal.aborted) return;
      if (response.error || !response.data) throw new Error(response.error || fallback);
      list.items = response.data;
      list.status = 'ready';
    } catch (cause) {
      if (!request.signal.aborted) {
        list.error = getApiErrorMessage(cause);
        list.status = 'error';
      }
    } finally {
      if (requests.get(list) === request) requests.delete(list);
    }
  }

  function loadProjects() {
    return load(
      projects,
      (signal) => projectsApi.listProjects({ signal }),
      'Не удалось загрузить список проектов.',
    );
  }

  function loadNotes(projectId: string) {
    if (disposed) return;
    let list = notesByProject.get(projectId);
    if (!list) {
      list = reactive(workspaceList<NoteListItemResponse>());
      notesByProject.set(projectId, list);
    }
    return load(
      list,
      (signal) => notesApi.listNotes(projectId, { signal }),
      'Не удалось загрузить заметки.',
    );
  }

  function restartPending(list: WorkspaceList<unknown>, reload: () => Promise<void> | undefined) {
    const request = requests.get(list);
    if (!request) return;
    // Ответ списка, запрошенного до сохранения, может содержать устаревшие данные.
    request.abort();
    requests.delete(list);
    void reload();
  }

  function saveProject(project: ProjectResponse) {
    if (disposed) return;
    const position = projects.items.findIndex((item) => item.id === project.id);
    if (position === -1) projects.items.unshift(project);
    else projects.items.splice(position, 1, project);
    restartPending(projects, loadProjects);
  }

  function saveNote(projectId: string, note: NoteListItemResponse) {
    if (disposed) return;
    const list = notesByProject.get(projectId);
    if (!list) return;
    list.items = [note, ...list.items.filter((item) => item.id !== note.id)];
    restartPending(list, () => loadNotes(projectId));
  }

  const workspace = { projects, notesByProject, loadProjects, loadNotes, saveProject, saveNote };
  provide(workspaceKey, workspace);
  return workspace;
}

export function useWorkspace() {
  const workspace = inject(workspaceKey);
  if (!workspace) throw new Error('Рабочая область недоступна.');
  return workspace;
}
