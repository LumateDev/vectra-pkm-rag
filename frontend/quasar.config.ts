import { defineConfig } from '#q-app';

export default defineConfig(() => ({
  boot: ['theme'],
  css: ['app.scss'],
  extras: ['material-icons'],
  build: {
    typescript: { strict: true, vueShim: true },
    vueRouterMode: 'history',
    vitePlugins: [
      [
        'vite-plugin-checker',
        {
          vueTsc: true,
          eslint: {
            lintCommand: 'eslint -c ./eslint.config.js "./src*/**/*.{ts,js,mjs,cjs,vue}"',
            useFlatConfig: true,
          },
        },
        { server: false },
      ],
    ],
  },
  devServer: { open: false, port: 9000 },
  framework: {
    lang: 'ru',
    plugins: ['Meta'],
  },
}));
