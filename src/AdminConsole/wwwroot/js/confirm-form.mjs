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
                                  
            // Not sure, but I think we need to run validate.
            form.checkValidity();

            const submitterName = event.submitter?.getAttribute("name");
            let submitterInput = null;
            if(submitterName) {
                // pollyfil form submittion with submitter button name/value
                submitterInput = document.createElement('input');
                submitterInput.type = 'hidden';
                submitterInput.name = submitterName;
                submitterInput.value = event.submitter?.getAttribute("value");                
            }
            
            // show the modal and set copy            
            showModal({
                show: true,
                onConfirm: () => {
                    // add submitter name/value to form (submitting forms programmatically does not include the submitter button name/value)
                    form.appendChild(submitterInput);
                    form.submit();
                },
                title: event.submitter.getAttribute('confirm-title') || 'Are you really sure?'
            });            
            
            
            return false;
        });
    });
});