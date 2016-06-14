Flow of Program

//A description of the method calls made by the Roslyn APIs
//The calls are sequential and indentation refers to those functions being called within the higher method
//Most methods do not have the attached code here

1.	Setting up the script execution environment


state = await CSharpScript.RunAsync("",
         ScriptOptions.Default.WithImports("System.IO"));


public static Task<ScriptState<object>> RunAsync(string code, ScriptOptions options = null, object globals = null, Type globalsType = null, CancellationToken cancellationToken = default(CancellationToken))
{
    return RunAsync<object>(code, options, globals, globalsType, cancellationToken);
}


public static Task<ScriptState<T>> RunAsync<T>(string code, ScriptOptions options = null, object globals = null, Type globalsType = null, CancellationToken cancellationToken = default(CancellationToken))
{
    return Create<T>(code, options, globalsType ?? globals?.GetType()).RunAsync(globals, cancellationToken);
}

        public static Script<T> Create<T>(string code, ScriptOptions options = null, Type globalsType = null, InteractiveAssemblyLoader assemblyLoader = null)
        {
            return Script.CreateInitialScript<T>(CSharpScriptCompiler.Instance, code, options, globalsType, assemblyLoader);
        }


        internal static Script<T> CreateInitialScript<T>(ScriptCompiler compiler, string codeOpt, ScriptOptions optionsOpt, Type globalsTypeOpt, InteractiveAssemblyLoader assemblyLoaderOpt)
        {
            return new Script<T>(compiler, new ScriptBuilder(assemblyLoaderOpt ?? new InteractiveAssemblyLoader()), codeOpt ?? "", optionsOpt ?? ScriptOptions.Default, globalsTypeOpt, previousOpt: null);
        }


public new Task<ScriptState<T>> RunAsync(object globals, CancellationToken cancellationToken)
            => RunAsync(globals, null, cancellationToken);


public new Task<ScriptState<T>> RunAsync(object globals = null, Func<Exception, bool> catchException = null, CancellationToken cancellationToken = default(CancellationToken))
{
    // The following validation and executor construction may throw;
    // do so synchronously so that the exception is not wrapped in the task.

    ValidateGlobals(globals, GlobalsType);

    var executionState = ScriptExecutionState.Create(globals);
    var precedingExecutors = GetPrecedingExecutors(cancellationToken);
    var currentExecutor = GetExecutor(cancellationToken);

    return RunSubmissionsAsync(executionState, precedingExecutors, currentExecutor, catchException, cancellationToken);
}


internal async Task<TResult> RunSubmissionsAsync<TResult>(
            ImmutableArray<Func<object[], Task>> precedingExecutors,
            Func<object[], Task> currentExecutor,
            StrongBox<Exception> exceptionHolderOpt,
            Func<Exception, bool> catchExceptionOpt,
            CancellationToken cancellationToken)
{		}


2. Running a command
int i = 5;

state = await state.ContinueWithAsync(dispText);

public Task<ScriptState<object>> ContinueWithAsync(string code, ScriptOptions options = null, Func<Exception, bool> catchException = null, CancellationToken cancellationToken = default(CancellationToken))
            => Script.ContinueWith<object>(code, options).RunFromAsync(this, catchException, cancellationToken);

        public Script<TResult> ContinueWith<TResult>(string code, ScriptOptions options = null) =>
            new Script<TResult>(Compiler, Builder, code ?? "", options ?? InheritOptions(Options), GlobalsType, this);


public new Task<ScriptState<T>> RunFromAsync(ScriptState previousState, Func<Exception, bool> catchException = null, CancellationToken cancellationToken = default(CancellationToken))
{		}


        private ImmutableArray<Func<object[], Task>> TryGetPrecedingExecutors(Script lastExecutedScriptInChainOpt, CancellationToken cancellationToken)
        {		}


        private Func<object[], Task<T>> GetExecutor(CancellationToken cancellationToken)
        {
            if (_lazyExecutor == null)
            {
                Interlocked.CompareExchange(ref _lazyExecutor, Builder.CreateExecutor<T>(Compiler, GetCompilation(), cancellationToken), null);
            }

            return _lazyExecutor;
        }

				public Compilation GetCompilation()
       			{
    				if (_lazyCompilation == null)
    				{
        				var compilation = Compiler.CreateSubmission(this);
        				Interlocked.CompareExchange(ref _lazyCompilation, compilation, null);
    				}
    				return _lazyCompilation;
                }

                        public override Compilation CreateSubmission(Script script)
                        {/*builds compilation using Roslyn APIs*/}	

	

                internal Func<object[], Task<T>> CreateExecutor<T>(ScriptCompiler compiler, Compilation compilation, CancellationToken cancellationToken)
                {	}


                        private static void ThrowIfAnyCompilationErrors(DiagnosticBag diagnostics, DiagnosticFormatter formatter)
                        {       }


                        // Builds a delegate that will execute just this script's code.
                        private Func<object[], Task<T>> Build<T>(
                            Compilation compilation,
                            DiagnosticBag diagnostics,
                            CancellationToken cancellationToken)
                        {
                            var entryPoint = compilation.GetEntryPoint(cancellationToken);

                            using (var peStream = new MemoryStream())
                            {
                                var emitResult = compilation.Emit(
                                    peStream: peStream,
                                    pdbStream: null,
                                    xmlDocumentationStream: null,
                                    win32Resources: null,
                                    manifestResources: null,
                                    options: EmitOptions.Default,
                                    cancellationToken: cancellationToken);

                                diagnostics.AddRange(emitResult.Diagnostics);

                                if (!emitResult.Success)
                                {
                                    return null;
                                }

                                // let the loader know where to find assemblies:
                                foreach (var referencedAssembly in compilation.GetBoundReferenceManager().GetReferencedAssemblies())
                                {
                                    var path = (referencedAssembly.Key as PortableExecutableReference)?.FilePath;
                                    if (path != null)
                                    {
                                        // TODO: Should the #r resolver return contract metadata and runtime assembly path -
                                        // Contract assembly used in the compiler, RT assembly path here.
                                        _assemblyLoader.RegisterDependency(referencedAssembly.Value.Identity, path);
                                    }
                                }

                                peStream.Position = 0;

                                var assembly = _assemblyLoader.LoadAssemblyFromStream(peStream, pdbStream: null);
                                var runtimeEntryPoint = GetEntryPointRuntimeMethod(entryPoint, assembly, cancellationToken);

                                return runtimeEntryPoint.CreateDelegate<Func<object[], Task<T>>>();
                            }
                        }


                                internal static MethodInfo GetEntryPointRuntimeMethod(IMethodSymbol entryPoint, Assembly assembly, CancellationToken cancellationToken)
                                {
                                    string entryPointTypeName = MetadataHelpers.BuildQualifiedName(entryPoint.ContainingNamespace.MetadataName, entryPoint.ContainingType.MetadataName);
                                    string entryPointMethodName = entryPoint.MetadataName;

                                    var entryPointType = assembly.GetType(entryPointTypeName, throwOnError: true, ignoreCase: false).GetTypeInfo();
                                    return entryPointType.GetDeclaredMethod(entryPointMethodName);
                                }


                        private static void ThrowIfAnyCompilationErrors(DiagnosticBag diagnostics, DiagnosticFormatter formatter)
                        {       }


        public ScriptExecutionState FreezeAndClone() 
        {//...
            return new ScriptExecutionState(/*...*/);
        }


                private ScriptExecutionState(object[] submissionStates, int count)
                {
                    _submissionStates = submissionStates;
                    _count = count;
               }


		
        private async Task<ScriptState<T>> RunSubmissionsAsync(
            ScriptExecutionState executionState,
            ImmutableArray<Func<object[], Task>> precedingExecutors, 
            Func<object[], Task> currentExecutor, 
            Func<Exception, bool> catchExceptionOpt,
            CancellationToken cancellationToken)
        {
            var exceptionOpt = (catchExceptionOpt != null) ? new StrongBox<Exception>() : null;
            T result = await executionState.RunSubmissionsAsync<T>(precedingExecutors, currentExecutor, exceptionOpt, catchExceptionOpt, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
            return new ScriptState<T>(executionState, this, result, exceptionOpt?.Value);
        }


                internal async Task<TResult> RunSubmissionsAsync<TResult>(
                   	 ImmutableArray<Func<object[], Task>> precedingExecutors,
                  	 Func<object[], Task> currentExecutor,
                   	 StrongBox<Exception> exceptionHolderOpt,
                  	 Func<Exception, bool> catchExceptionOpt,
                  	 CancellationToken cancellationToken)
            	{
                    Debug.Assert(_frozen == 0);
                    Debug.Assert((exceptionHolderOpt != null) == (catchExceptionOpt != null));

                    // Each executor points to a <Factory> method of the Submission class.
                    // The method creates an instance of the Submission class passing the submission states to its constructor.
                    // The consturctor initializes the links between submissions and stores the Submission instance to 
                    // a slot in submission states that corresponds to the submission.
                    // The <Factory> method then calls the <Initialize> method that consists of top-level script code statements.

                    int executorIndex = 0;
                    try
                    {
                        while (executorIndex < precedingExecutors.Length)
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            EnsureStateCapacity();

                            try
                            {
                                await precedingExecutors[executorIndex++](_submissionStates).ConfigureAwait(continueOnCapturedContext: false);
                            }
                            finally
                            {
                                // The submission constructor always runs into completion (unless we emitted bad code).
                                // We need to advance the counter to reflect the updates to submission states done in the constructor.
                                AdvanceStateCounter();
                            }
                        }

                        cancellationToken.ThrowIfCancellationRequested();

                        TResult result;
                        EnsureStateCapacity();

                        try
                        {
                            executorIndex++;
                            result = await ((Task<TResult>)currentExecutor(_submissionStates)).ConfigureAwait(continueOnCapturedContext: false);
                        }
                        finally
                        {
                            // The submission constructor always runs into completion (unless we emitted bad code).
                            // We need to advance the counter to reflect the updates to submission states done in the constructor.
                            AdvanceStateCounter();
                        }

                        return result;
                    }
                    catch (Exception exception) when (catchExceptionOpt?.Invoke(exception) == true)
                    {
                        // The following code creates instances of all submissions without executing the user code.
                        // The constructors don't contain any user code.
                        var submissionCtorArgs = new object[] { null };

                        while (executorIndex < precedingExecutors.Length)
                        {
                            EnsureStateCapacity();

                            // update the value since the array might have been resized:
                            submissionCtorArgs[0] = _submissionStates;

                            Activator.CreateInstance(precedingExecutors[executorIndex++].GetMethodInfo().DeclaringType, submissionCtorArgs);
                            AdvanceStateCounter();
                        }

                        if (executorIndex == precedingExecutors.Length)
                        {
                            EnsureStateCapacity();

                            // update the value since the array might have been resized:
                            submissionCtorArgs[0] = _submissionStates;

                            Activator.CreateInstance(currentExecutor.GetMethodInfo().DeclaringType, submissionCtorArgs);
                            AdvanceStateCounter();
                        }

                        exceptionHolderOpt.Value = exception;
                        return default(TResult);
                    }
                }

        			
                        private void EnsureStateCapacity()
                        {
                            // make sure there is enough free space for the submission to add its state
                            if (_count >= _submissionStates.Length)
                            {
                                Array.Resize(ref _submissionStates, Math.Max(_count, _submissionStates.Length * 2));
                            }
                        }


StoreOutput(state.ReturnValue); //And we have the return value of the command(s) given as input