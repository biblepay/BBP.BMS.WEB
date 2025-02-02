      (function () {
  'use strict';

  //!autoLanguage

  const {EditorState, Compartment} = CM["@codemirror/state"];
  const {htmlLanguage, html} = CM["@codemirror/lang-html"];
  const {language} = CM["@codemirror/language"];
  const {javascript} = CM["@codemirror/lang-javascript"];

  const languageConf = new Compartment;

  const autoLanguage = EditorState.transactionExtender.of(tr => {
    if (!tr.docChanged) return null
    let docIsHTML = /^\s*</.test(tr.newDoc.sliceString(0, 100));
    let stateIsHTML = tr.startState.facet(language) == htmlLanguage;
    if (docIsHTML == stateIsHTML) return null
    return {
      effects: languageConf.reconfigure(docIsHTML ? html() : javascript())
    }
  });

  //!enable

  const {EditorView, basicSetup} = CM["codemirror"];

  new EditorView({
    doc: 'console.log("hello")',
    extensions: [
      basicSetup,
      languageConf.of(javascript()),
      autoLanguage
    ],
    parent: document.querySelector("#editor")
  });

})();
    