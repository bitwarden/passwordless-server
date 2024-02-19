import {ref, computed} from "vue";

const active = ref(false);
const title = ref(null);
const form = ref(null);

export const showModal = (props) => {
    active.value = props.show;
    form.value = props.form;
    title.value = props.title;
} 

export default {
    template: /*html*/`
      <div id="modal-div" v-if="active" aria-labelledby="modal-title" role="dialog" aria-modal="true">
        <div class="fixed inset-0 bg-gray-500 bg-opacity-75 transition-opacity"></div>

        <div class="fixed inset-0 z-10 w-screen overflow-y-auto">
          <div class="flex min-h-full items-end justify-center p-4 text-center sm:items-center sm:p-0">
            <div class="relative transform overflow-hidden rounded-lg bg-white px-4 pb-4 pt-5 text-left shadow-xl transition-all sm:my-8 sm:w-full sm:max-w-lg sm:p-6">
              <div class="sm:flex sm:items-start">
                <div class="mx-auto flex h-12 w-12 flex-shrink-0 items-center justify-center rounded-full bg-red-100 sm:mx-0 sm:h-10 sm:w-10">
                  <svg class="h-6 w-6 text-red-600" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" aria-hidden="true">
                    <path stroke-linecap="round" stroke-linejoin="round" d="M12 9v3.75m-9.303 3.376c-.866 1.5.217 3.374 1.948 3.374h14.71c1.73 0 2.813-1.874 1.948-3.374L13.949 3.378c-.866-1.5-3.032-1.5-3.898 0L2.697 16.126zM12 15.75h.007v.008H12v-.008z" />
                  </svg>
                </div>
                <div class="text-center sm:ml-4 sm:text-left">
                  <h3 class="text-base font-semibold leading-6 text-gray-900">{{title || "Are you sure?"}}</h3>
                  <div class="mt-2">
                    <p class="text-sm text-gray-500">Please confirm your action</p>
                  </div>
                </div>
              </div>
              <div class="mt-5 sm:ml-10 sm:mt-4 sm:flex sm:pl-4">
                <button @click="confirm" type="button" class="inline-flex w-full justify-center rounded-md bg-red-600 px-3 py-2 text-sm font-semibold text-white shadow-sm hover:bg-red-500 sm:w-auto">Confirm</button>
                <button @click="deny" type="button" class="mt-3 inline-flex w-full justify-center rounded-md bg-white px-3 py-2 text-sm font-semibold text-gray-900 shadow-sm ring-1 ring-inset ring-gray-300 hover:bg-gray-50 sm:ml-3 sm:mt-0 sm:w-auto">Cancel</button>
              </div>
            </div>
          </div>
        </div>
      </div>
    `,    
    setup(props) {               
        const confirm = () => {            
            form.value.submit();
            active.value = false;
        };
        const deny = () => {
            active.value = false;            
        };
        
        return {
            active,
            title,
            confirm,
            deny
        }
    }
}