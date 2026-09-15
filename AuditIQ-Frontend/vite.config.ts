/// <reference types="vitest/config" />
import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'
import path from 'node:path'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: {
      '@': path.resolve(import.meta.dirname, './src'),
    },
  },
  server: {
    port: 5173,
    // Proxies API calls to the AuditIQ.Api dev server (see backend
    // Properties/launchSettings.json — "http" profile, port 5224) so the
    // frontend can call relative /api paths without CORS friction in dev.
    // Production still calls the API's real base URL via VITE_API_BASE_URL,
    // since the two apps deploy separately (Section 2/12.C).
    proxy: {
      '/api': { target: 'http://localhost:5224', changeOrigin: true },
    },
  },
  test: {
    environment: 'jsdom',
    setupFiles: ['./src/test/setup.ts'],
    globals: true,
    // 'forks' (Vitest's default pool) fails to spawn worker processes in this
    // sandboxed Windows environment; 'threads' avoids the child-process spawn.
    pool: 'threads',
  },
})
