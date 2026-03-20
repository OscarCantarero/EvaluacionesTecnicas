/** @type {import('tailwindcss').Config} */
module.exports = {
  content: ['./src/**/*.{html,ts}'],
  theme: {
    extend: {}
  },
  plugins: [require('daisyui')],
  daisyui: {
    themes: [
      {
        light: {
          primary: '#1E386E',
          secondary: '#32406C',
          accent: '#963D4C',
          neutral: '#000000',
          'base-100': '#ffffff',
          'base-200': '#f3f5f9',
          'base-300': '#e5e9f2',
          info: '#1E386E',
          success: '#1E386E',
          warning: '#963D4C',
          error: '#BD2F37'
        }
      },
      {
        dark: {
          primary: '#4a6aaf',
          secondary: '#5a6a9c',
          accent: '#c27585',
          neutral: '#1a2236',
          'base-100': '#111827',
          'base-200': '#0b1324',
          'base-300': '#1f2a44',
          info: '#4a6aaf',
          success: '#4a6aaf',
          warning: '#c27585',
          error: '#e06070'
        }
      }
    ]
  }
};
