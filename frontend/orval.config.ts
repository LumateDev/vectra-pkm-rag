import { existsSync } from 'node:fs';
import { loadEnvFile } from 'node:process';
import { defineConfig } from 'orval';

if (existsSync('.env')) loadEnvFile('.env');

export default defineConfig({
  vectra: {
    input: {
      target: process.env.SWAGGER_URL?.trim() || 'https://127.0.0.1:7000/swagger/v1/swagger.json',
    },
    output: {
      mode: 'tags-split',
      target: './src/api/generated/endpoints',
      schemas: './src/api/generated/models',
      client: 'axios',
      clean: true,
      indexFiles: true,
      override: {
        enumGenerationType: 'const',
        mutator: {
          path: './src/api/axios-instance.ts',
          name: 'customAxiosInstance',
        },
      },
    },
    hooks: { afterAllFilesWrite: 'prettier --write' },
  },
});
