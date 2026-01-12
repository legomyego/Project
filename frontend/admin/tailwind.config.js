/** @type {import('tailwindcss').Config} */
export default {
  // Dark mode configuration - uses class strategy for manual control
  darkMode: ["class"],

  // Content paths for Tailwind to scan for class names
  content: [
    "./index.html",
    "./src/**/*.{js,ts,jsx,tsx}",
  ],

  theme: {
    extend: {},
  },

  plugins: [],
}
