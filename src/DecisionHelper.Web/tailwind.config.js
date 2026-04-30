/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    './Components/**/*.razor',
    './wwwroot/js/**/*.js',
  ],
  safelist: [
    // Dynamic Tone parameter on Quadrant / ManualQuadrant components.
    'text-emerald-700',
    'text-rose-700',
    'text-zinc-700',
  ],
  theme: {
    extend: {},
  },
  plugins: [],
};
