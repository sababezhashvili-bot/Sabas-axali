<template>
  <router-view />
</template>

<style>
@import url('https://fonts.cdnfonts.com/css/sf-pro-display');
@import url('https://fonts.googleapis.com/css2?family=Noto+Sans+Georgian:wght@100..900&display=swap');

:root {
  --brand-green: #72A98F;
  --accent: var(--brand-green);
}

body {
  margin: 0;
  padding: 0;
  font-family: "Montserrat", "SF Pro Display", "Noto Sans Georgian", sans-serif;
  background: #000;
  color: #fff;
  overflow: hidden;
  letter-spacing: 0.05em; /* Airy and luxurious feel */
}

* {
  box-sizing: border-box;
}

.material-symbols-outlined {
  font-variation-settings: 'wght' 200;
  font-size: 18px;
  color: #fff !important;
}

/* Global Utility Classes */
.glass-panel {
    background: rgba(255, 255, 255, 0.1);
    backdrop-filter: blur(10px);
    -webkit-backdrop-filter: blur(10px);
    border: 1px solid rgba(255, 255, 255, 0.2);
    border-radius: 15px;
    color: white;
}

.accent-color {
    color: #72A98F;
}
</style>
