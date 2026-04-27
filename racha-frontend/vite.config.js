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
    chunkSizeWarningLimit: 3000,
    target: 'esnext',
    rollupOptions: {
      output: {
        manualChunks: {
          // Mapbox is large — separate chunk so app shell loads fast
          'mapbox': ['mapbox-gl'],
          // Mapbox geocoder — only loaded on map page
          'geocoder': ['@mapbox/mapbox-gl-geocoder'],
          // Vue runtime
          'vue': ['vue', 'vue-router'],
        }
      }
    }
  }
})
