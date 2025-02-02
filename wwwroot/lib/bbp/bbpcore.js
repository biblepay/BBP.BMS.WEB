

        window.onload = async function ()
        {
            /*
            var erc20sig = getCookie('erc20signature');
            var erc20address = getCookie('erc20address');
            var fUseMetamask = !(window.location.href.indexOf("localhost") > -1);
            console.log('use metamask ' + fUseMetamask);
            if (!fUseMetamask) {
                return;
            }

            // Continue to use metamask here:
            if (erc20sig == null || erc20address == null || erc20sig == '' || erc20address == '') {
                if (typeof window.ethereum !== 'undefined') {
                    console.log('MetaMask is installed.');
                    await ERC712Authentication(finishedAuthenticating);

                }
                else {
                    alert('Sorry, to use BiblePay Unchained you must authenticate with metamask.  Please install metamask first then come back to log in. ');
                }
            }
            else {
                console.log('We have detected an authenticated user...');
            }
            erc20sig = getCookie('erc20signature');
            erc20address = getCookie('erc20address');
            console.log('addr ' + erc20address + ' .sig. ' + erc20sig);
            if (erc20sig != null && erc20address != null) {
                DoCallback('Profile_Authenticate', null, 'profile/processdocallback');
            }
            */

        }



async function finishedAuthenticating(err, result) {

    try {
        if (!result) {
            alert('We didnt receive anything back from metamask, therefore you will not be able to authenticate.  Ensure you have updated metamask to the latest version by visiting their plugin site and upgrading.  It could be that your version does not support v3 signatures. ');
            return;
        }
        if (result.error) {
            console.error('ERROR', result);
            console.error(err);
            alert('Unable to sign authorization packet.');
            return "";
        }
        console.log('[v3] SIGNED BBP AUTH PACKET : ' + erc20address + "," + JSON.stringify(result.result));
        setCookie('erc20address', erc20address, 30);
        setCookie('erc20signature', result.result, 30);
        DoCallback('Profile_Authenticate_Full', null, 'profile/processdocallback');

        return result.result;
    } catch (e) {
        alert('(2) We didnt receive anything back from metamask, therefore you will not be able to authenticate.  Ensure you have updated metamask to the latest version by visiting their plugin site and upgrading.  It could be that your version does not support v3 signatures. ');
        return;
    }
}


        async function getAccount() {
            const accounts = await ethereum.request({ method: 'eth_requestAccounts' });
            const account = accounts[0];
            console.log(account);
        }



        var erc20address;
        async function ERC712Authentication(oCallbackFunction)
        {
            const msgParams = JSON.stringify({
                domain: {
                    chainId: 0,
                    // Give a user friendly name to the specific contract you are signing for.
                    name: 'BiblePay Unchained',
                    verifyingContract: '0x0',
                    version: '1',
                },
                message: {
                    contents: 'Please sign this message to log into BiblePay Unchained.',
                    from: {
                        name: 'BiblePay Unchained',
                    }
                },
                primaryType: 'BiblePayAuthenticator',
                types: {
                    EIP712Domain: [
                        { name: 'name', type: 'string' },
                        { name: 'version', type: 'string' },
                        { name: 'chainId', type: 'uint256' },
                        { name: 'verifyingContract', type: 'address' },
                    ],
                    BiblePayAuthenticator: [
                        { name: 'from', type: 'Person' },
                        { name: 'contents', type: 'string' },
                    ],
                    Person: [
                        { name: 'name', type: 'string' },
                    ],
                },
            });


            const accounts = await ethereum.request({ method: 'eth_requestAccounts' });
            erc20address = accounts[0];
            console.log(msgParams);
            console.log(erc20address);
            var params = [erc20address, msgParams];
            var method = 'eth_signTypedData_v4';
            await web3.currentProvider.sendAsync(
                    {
                        
                        method,
                        params,
                        erc20address,
                }, oCallbackFunction
            );
            return "";
        }



