import {ref, computed} from "vue";

export default {
    template: /*html*/`
    <div class="mt-3">
        <button v-if="!showConfirmation" id="btn-delete-organization" class="btn-danger disabled:opacity-25" :disabled="disableDelete" @click="enableConfirmation">Delete this organization</button>
        <template v-if="showConfirmation" class="flex flex-col">
            <div class="my-2 flex rounded-md shadow-sm">
                <input required placeholder="Name of organization" type="text" v-model="confirmInputValue" class="block font-mono rounded-none rounded-l-md border-0 py-1.5 pl-2 text-gray-900 ring-1 ring-inset ring-gray-300 placeholder:text-gray-400 focus:ring-2 focus:ring-inset focus:ring-indigo-600 sm:text-sm sm:leading-6">
                <button title="I'm sure, delete the organization" class="btn-danger rounded-l-none disabled:opacity-25" type="submit" :disabled="disableConfirmation">I'm Sure</button>
            </div>                        
            <p class="italic mt-3">Type name of organization and click "I'm Sure"</p>
        </template>
    </div>
    `,
    props: ['id'],
    setup(props) {
        const disableDelete = ref(false);
        const showConfirmation = ref(false);
        const confirmInputValue = ref('');

        const disableConfirmation = computed(() => {
            return props.id !== confirmInputValue.value;
        });

        const enableConfirmation = function () {
            showConfirmation.value = true;
            disableDelete.value = true;
        }

        return {
            disableDelete,
            showConfirmation,
            disableConfirmation,
            confirmInputValue,
            enableConfirmation
        }
    }
}