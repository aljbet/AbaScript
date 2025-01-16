using LLVMSharp.Interop;

namespace AbaScript.LlvmCompilerClasses;

public record ArrayAllocaInfo(LLVMValueRef Alloca, LLVMTypeRef Ty, ulong Size) 
    : AllocaInfo(Alloca, Ty);