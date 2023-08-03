import { ref, computed } from "vue";

export default {
    template: /*html*/`
    <div class="mt-3">
        <button id="btn-delete-app" class="btn-danger disabled:opacity-25" :disabled="disableDelete" @click="enableConfirmation">Delete Application</button>
        <template v-if="showConfirmation" class="flex flex-col">
            <input required placeholder="Name of application" class="ml-3 rounded-md border bg-white shadow-sm focus:outline-none" v-model="confirmInputValue"/>
            <button id="btn-confirm-delete" class="ml-3 btn-primary disabled:opacity-25" type="submit" :disabled="disableConfirmation">I'm Sure</button>
            <p class="italic mt-3">Type name of application and click "I'm Sure"</p>
        </template>
    </div>
    `,
    props: ['appId'],
    setup(props) {
        const disableDelete = ref(false);
        const showConfirmation = ref(false);
        const confirmInputValue = ref('');
        
        const disableConfirmation = computed(() => {
            return props.appId !== confirmInputValue.value;
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