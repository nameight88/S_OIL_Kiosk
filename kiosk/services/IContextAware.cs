namespace s_oil.Services
{
    public interface IContextAware
    {   
        void SetContext(s_oil.Services.ApplicationContext context);  // 전체 네임스페이스 명시
    }   
}