import { defineCustomElement } from 'vue'

const LocalTimeElement = defineCustomElement({
    props: {
        datetime: String,
    },
    computed: {
        localTime() { return new Date(this.datetime).toLocaleString() }
    },
    template: /*html*/`
    <time :datetime="datetime">{{ localTime }}</time>
  `,
    styles: [``]
})

customElements.define('local-time', LocalTimeElement)