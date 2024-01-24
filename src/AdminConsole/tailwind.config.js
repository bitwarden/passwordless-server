function rgba(color) {
    return "rgb(var(" + color + ") / <alpha-value>)";
}

/** @type {import('tailwindcss').Config} */
module.exports = {
    content: [
        "./Pages/**/*.{razor,html,cshtml}",
        "./wwwroot/js/**/*.{js,mjs}",
        "./TagHelpers/**/*.cs",
        "./Components/**/*.razor"
    ],
    safelist: [
        'validation-summary-errors',
        'input-validation-error',
        'field-validation-error',
    ],
    theme: {
        extend: {
            colors: {
                primary: {
                    300: rgba("--color-primary-300"),
                    500: rgba("--color-primary-500"),
                    700: rgba("--color-primary-700"),
                },
            },
        },
    },
    plugins: [
        require('@tailwindcss/aspect-ratio'),
        require('@tailwindcss/forms'),
        require('@tailwindcss/typography')
    ],
}
