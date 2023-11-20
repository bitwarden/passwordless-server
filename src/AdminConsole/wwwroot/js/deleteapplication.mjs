import {ref, computed} from "vue";

export default {
    template: /*html*/`
    <div class="mt-3">
        <button type="button" v-if="!showConfirmation" id="btn-delete-app" class="btn-secondary disabled:opacity-25" :disabled="disableDelete" @click="enableConfirmation">Delete Application</button>
        <template v-if="showConfirmation" class="flex flex-col">
            <div class="my-2 flex rounded-md shadow-sm">
                <input required placeholder="Name of application" type="text" v-model="confirmInputValue" class="text-input font-mono rounded-r-none" />
                <button title="I'm sure, delete the applications" class="btn-danger rounded-l-none disabled:opacity-25" type="submit" :disabled="disableConfirmation">I'm Sure</button>
            </div>                        
            <p class="italic mt-3">Type '{{appId}}' and click "I'm Sure"</p>
        </template>
    </div>
    `,
    props: ['appId'],
    setup(props) {
        const disableDelete = ref(false);
        const showConfirmation = ref(false);
        const confirmInputValue = ref('');
        const appId = ref(props.appId);

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
            enableConfirmation,
            appId
        }
    }
}