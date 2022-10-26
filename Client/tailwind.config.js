/** @type {import('tailwindcss').Config} */
module.exports = {
    content: [
        "./Pages/*.razor",
        "./Pages/**/*.razor",
        "./Shared/*.razor",
        "./Components/*.razor",
        "./App.razor"
    ],
    theme: {
        container: {
            center: true,
            padding: '2rem'
        },
        extend: {
            spacing: {
                '112': '28rem',
                '128': '32rem',
                '144': '36rem',
            },
            height: {
                '112': '28rem',
                '128': '32rem',
                '144': '36rem',
                '160': '40rem'
            },
            borderRadius: {
                '4xl': '2rem',
            }
        },
  },
  plugins: [],
}
