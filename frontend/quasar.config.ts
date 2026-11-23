import { defineConfig } from '#q-app';

const apiProxyTarget = new URL(import.meta.env.API_PROXY_TARGET || 'https://127.0.0.1:7000');

export default defineConfig(() => ({
  boot: ['theme', 'api'],
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
  devServer: {
    open: false,
    host: '127.0.0.1',
    port: 9000,
    strictPort: true,
    https: true,
    proxy: {
      '/auth/': {
        target: apiProxyTarget.href,
        changeOrigin: true,
        // Kestrel использует локальный сертификат разработки.
        secure: apiProxyTarget.hostname !== '127.0.0.1',
      },
      '^/projects(?:[/?]|$)': {
        target: apiProxyTarget.href,
        changeOrigin: true,
        secure: apiProxyTarget.hostname !== '127.0.0.1',
        bypass(request) {
          if (request.method === 'GET' && request.headers.accept?.includes('text/html')) {
            return request.url;
          }
        },
      },
    },
  },
  framework: {
    lang: 'ru',
    plugins: ['Meta', 'Notify'],
  },
}));
