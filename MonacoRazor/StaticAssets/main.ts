import * as monaco from 'monaco-editor';

type DotNetObjectReference = {
    invokeMethodAsync: (methodName: string, ...args: any[]) => Promise<void>;
};

const registeredCompletionHandlers: Record<string, boolean> = {};

// Store the editor instance in a WeakMap keyed by the HTML element
const editorMap = new WeakMap<HTMLElement, monaco.editor.IStandaloneCodeEditor>();

export function init(
    element: HTMLElement,
    component: DotNetObjectReference,
    language: string,
    value: string
) {
    // Register completion provider once per language if needed
    if (!registeredCompletionHandlers[language]) {
        registeredCompletionHandlers[language] = true;
        monaco.languages.registerCompletionItemProvider(language, new RemoteCompletionItemProvider());
    }

    // Create the Monaco editor
    const editor = monaco.editor.create(element, {
        value,
        language,
        minimap: { enabled: false }
    });

    // Associate the Blazor component with the editor's model (cast to `any` to avoid type errors)
    (editor.getModel() as any).blazorComponent = component;

    // Example event: handle blur
    editor.onDidBlurEditorText(() => {
        const currentValue = editor.getValue();
        component.invokeMethodAsync('HandleEditorBlur', currentValue);
    });

    // Keep track of the editor instance so we can update it later
    editorMap.set(element, editor);
}

export function updateValue(
    element: HTMLElement,
    component: DotNetObjectReference,
    value: string
) {
    // Retrieve the editor instance from the WeakMap
    const editor = editorMap.get(element);
    if (editor) {
        editor.setValue(value);
    }
}

// Get the curent value of the editor
export function getValue(element: HTMLElement) {
    const editor = editorMap.get(element);
    return editor ? editor.getValue() : '';
}

class RemoteCompletionItemProvider implements monaco.languages.CompletionItemProvider {
    public triggerCharacters = ['.'];

    async provideCompletionItems(
        model: monaco.editor.ITextModel,
        position: monaco.Position,
        context: monaco.languages.CompletionContext,
        token: monaco.CancellationToken
    ): Promise<monaco.languages.CompletionList> {

        // Retrieve the Blazor component reference from the model (cast as needed)
        const component = (model as any).blazorComponent;

        if (component) {
            // Call the .NET method to get completion suggestions
            const suggestions = await component.invokeMethodAsync('GetCompletions', model.getValue(), position);

            if (suggestions && Array.isArray(suggestions)) {
                // Map server-returned suggestions into Monaco's CompletionItem format
                const mappedSuggestions = suggestions.map((s: any): monaco.languages.CompletionItem => {
                    return {
                        kind: s.kind as monaco.languages.CompletionItemKind,  // cast or convert
                        label: s.label,
                        insertText: s.insertText,
                        range: {
                            startLineNumber: position.lineNumber,
                            startColumn: position.column,
                            endLineNumber: position.lineNumber,
                            endColumn: position.column
                        },
                        documentation: s.documentation as string // or cast as IMarkdownString
                    };
                });

                // Return them wrapped in a CompletionList
                return {
                    suggestions: mappedSuggestions
                };
            }
        }

        // If no suggestions found or no Blazor component, return empty list
        return { suggestions: [] };
    }
}
