import { createApp } from 'vue';
import Modal, { showModal } from './modal.mjs';

document.addEventListener('DOMContentLoaded', function () {
    
    // create the modal container
    const modalEl = document.createElement('div');
    modalEl.id = 'modal-container';
    document.body.appendChild(modalEl);

    var modal = createApp(Modal).mount(modalEl);
        
    // intercept forms
    document.querySelectorAll('[confirm-submit]').forEach(function (element) {        
        element.addEventListener('submit', function (event) {
            event.preventDefault();
            const form = event.target;
                        
            // add submitter name/value to form (submitting forms programmatically does not include the submitter button name/value)
            const submitter = event.submitter;
            if (submitter) {
                const submitterName = submitter.getAttribute('name');
                if (submitterName) {
                    const submitterValue = submitter.getAttribute('value');
                    const submitterInput = document.createElement('input');
                    submitterInput.type = 'hidden';
                    submitterInput.name = submitterName;
                    submitterInput.value = submitterValue;
                    form.appendChild(submitterInput);
                }
            }
                       
            // Not sure, but I think we need to run validate.
            form.checkValidity();
            
            // show the modal and set copy            
            showModal({
                show: true,
                form,
                title: event.submitter.getAttribute('confirm-title') || 'Are you really sure?'
            });            
            
            
            return false;
        });
    });
});