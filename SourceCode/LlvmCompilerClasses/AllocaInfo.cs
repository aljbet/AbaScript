using LLVMSharp.Interop;

namespace AbaScript.LlvmCompilerClasses;

public record AllocaInfo(LLVMValueRef Alloca, LLVMTypeRef Ty);