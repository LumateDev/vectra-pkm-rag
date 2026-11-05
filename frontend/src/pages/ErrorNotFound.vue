<script setup lang="ts">
import { useMeta } from 'quasar';

useMeta({ title: 'Страница не найдена · VectraPKM' });
</script>

<template>
  <section class="not-found" aria-labelledby="not-found-title">
    <div class="not-found__code" aria-hidden="true">404<span class="not-found__dot">.</span></div>
    <div class="not-found__eyebrow">НЕМНОГО СБИЛИСЬ С ПУТИ</div>
    <h1 id="not-found-title">Здесь пока ничего нет</h1>
    <p>Кажется, эта страница потерялась.<br />Проверьте адрес или вернитесь к началу.</p>
    <q-btn
      class="not-found__action"
      color="primary"
      unelevated
      no-caps
      icon="arrow_back"
      :to="{ name: 'login' }"
      label="На страницу входа"
    />
    <span class="not-found__hint">Ваши знания на месте. Осталось найти дорогу.</span>
  </section>
</template>

<style scoped lang="scss">
@use '../css/theme/contract' as theme;

.not-found {
  width: 100%;
  max-width: 32.5rem;
  padding: 2.5rem;
  border: 1px solid theme.token('app-border');
  border-radius: 1.5rem;
  background: theme.token('app-surface');
  box-shadow: theme.token('app-card-shadow');
  text-align: center;

  &__code {
    margin-bottom: 1.75rem;
    font-size: clamp(5rem, 15vw, 7.5rem);
    font-weight: 700;
    line-height: 1;
    letter-spacing: -0.07em;
    color: theme.token('app-link');
  }

  &__dot {
    color: theme.token('app-muted');
  }

  &__eyebrow {
    color: theme.token('app-muted');
    font-size: 0.625rem;
    font-weight: 650;
    letter-spacing: 0.14em;
    line-height: 1.6;
  }

  h1 {
    margin: 0.75rem 0;
    color: theme.token('app-text');
    font-size: 1.75rem;
    font-weight: 600;
    line-height: 1.25;
    letter-spacing: -0.03em;
  }

  p {
    margin: 0;
    color: theme.token('app-muted');
    font-size: 0.875rem;
    line-height: 1.7;
  }

  &__action {
    margin-top: 1.75rem;
    min-height: 3rem;
    padding: 0.75rem 1.25rem;
    border-radius: 0.625rem;
    color: theme.token('app-on-primary');
    font-size: 0.875rem;

    :deep(.q-icon) {
      font-size: 1.125rem;
    }
  }

  &__hint {
    display: block;
    margin-top: 1.5rem;
    color: theme.token('app-muted');
    font-size: 0.75rem;
    line-height: 1.6;
  }
}

@media (max-width: 480px) {
  .not-found {
    padding: 2rem 1.25rem;

    h1 {
      font-size: 1.5rem;
    }
  }
}
</style>
