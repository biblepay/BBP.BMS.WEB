let currentCall; // call object (instance of class Call) with methods
let transferCall; // call object storing instance of transfer call
const logger = new Logger(document.getElementById('logarea')); // create instance of Logger with method write
const sdk = VoxImplant.getInstance();

// handle incoming call
sdk.on(VoxImplant.Events.IncomingCall, (e) => {
  handleIncomingCall(e);
});
