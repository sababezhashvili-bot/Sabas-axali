import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import path from 'path'

export default defineConfig({
  plugins: [vue()],
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src')
    }
  },
  server: {
    port: 3000,
    proxy: {
      '/api': {
        target: 'http://localhost:5216',
        changeOrigin: true,
      },
      '/images': {
        target: 'http://localhost:5216',
        changeOrigin: true,
      }
    }
  },
  build: {
    chunkSizeWarningLimit: 2000, // Silence the >500kb warning for Mapbox
  }
})
