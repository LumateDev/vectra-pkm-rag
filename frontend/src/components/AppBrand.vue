<template>
  <span class="brand">
    <span class="brand-symbol" aria-hidden="true">v.</span>
    <span>Vectra<span class="brand-suffix">PKM</span></span>
  </span>
</template>

<style scoped lang="scss">
@use '../css/theme/contract' as theme;

.brand {
  display: inline-flex;
  align-items: center;
  gap: 0.5625rem;
  color: theme.token('app-text');
  font-size: inherit;
  font-weight: 650;
  letter-spacing: -0.025em;
}

.brand-symbol {
  display: grid;
  place-items: center;
  width: 1.35em;
  height: 1.35em;
  background: theme.token('app-primary');
  border-radius: 0.625rem;
  color: theme.token('app-on-primary');
  font-size: 1.2em;
  line-height: 1;
}

.brand-suffix {
  color: theme.token('app-muted');
  font-weight: 400;
  margin-inline-start: 0.1em;
}
</style>
