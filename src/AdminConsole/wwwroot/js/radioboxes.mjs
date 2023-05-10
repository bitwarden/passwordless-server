import {ref} from 'vue';


export default {
    template: /*html*/`
  <div>  
  <fieldset>
<!--  <legend class="text-base font-semibold leading-6 text-gray-900">Select</legend>-->

  <div class="mt-4 grid grid-cols-1 gap-y-6 sm:grid-cols-3 sm:gap-x-4">
    <!--
      Checked: "border-transparent", Not Checked: "border-gray-300"
      Active: "border-indigo-600 ring-2 ring-indigo-600"
    -->
    <div v-for="item in items" >
    <label :class="{'border-transparent border-blue-600 ring-2 ring-blue-600':selected.id== item.id, 'cursor-not-allowed': item.disabled}"
    class="relative flex cursor-pointer rounded-lg border bg-white p-4 shadow-sm focus:outline-none" @click="clicker(item)">
      <input type="radio" name="project-type" value="Newsletter" class="sr-only" aria-labelledby="project-type-0-label" aria-describedby="project-type-0-description-0 project-type-0-description-1">
      <span class="flex flex-1">
        <span class="flex flex-col">
          <span id="project-type-0-label" class="block text-sm font-medium text-gray-900">{{item.title}}</span>
          <span id="project-type-0-description-0" class="mt-1 flex items-center text-sm text-gray-500">{{item.desc}}</span>
<!--          <span id="project-type-0-description-1" class="mt-6 text-sm font-medium text-gray-900">621 users</span>-->
        </span>
      </span>
      <!-- Not Checked: "invisible" -->
      <svg v-if="selected.id == item.id"  class="h-5 w-5 text-blue-600" viewBox="0 0 20 20" fill="currentColor" aria-hidden="true">
        <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.857-9.809a.75.75 0 00-1.214-.882l-3.483 4.79-1.88-1.88a.75.75 0 10-1.06 1.061l2.5 2.5a.75.75 0 001.137-.089l4-5.5z" clip-rule="evenodd" />
      </svg>
      <!--
        Active: "border", Not Active: "border-2"
        Checked: "border-blue-600", Not Checked: "border-transparent"
      -->
      <span class="pointer-events-none absolute -inset-px rounded-lg border-2" aria-hidden="true"></span>
    </label>
    </div>
  </div>
</fieldset>
</div>
  
`,
    props: ['items'],
    setup(props) {
        const items = props.items.map((item, index) => {
            return {
                id: index,
                title: item.title,
                desc: item.desc,
                disabled: item.disabled
            }
        });

        // const items = [
        //     {id: 1, title: 'Newsletter', description: 'Last message sent an hour ago', users: '621 users'},
        //     {id: 2, title: 'Existing Customers', description: 'Last message sent 2 weeks ago', users: '1200 users'},
        //     {id: 3, title: 'Trial Users', description: 'Last message sent 4 days ago', users: '2740 users'},
        // ];

        const selected = ref(items[0]);
        const clicker = (item) => {
            if(!item.disabled) {
                selected.value = item
            }
        }
        return {
            items,
            selected,
            clicker
        }
    }
}