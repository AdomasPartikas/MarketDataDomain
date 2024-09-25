using MarketDataDomain.API.Models;
using MarketDataDomain.API.Records;

namespace MarketDataDomain.API.Profile
{
    public class MarketDataProfile : AutoMapper.Profile
    {
        public MarketDataProfile()
        {
            CreateMap<FinnhubGroup, MarketDataDto>()
                // Stock Symbol Information
                .ForMember(dest => dest.Symbol, opt => opt.MapFrom(src => src.StockSymbols.Symbol))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.StockSymbols.Description))
                .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.StockSymbols.Currency))

                // Stock Quote Information
                .ForMember(dest => dest.CurrentPrice, opt => opt.MapFrom(src => src.Quotes.CurrentPrice))
                .ForMember(dest => dest.HighPrice, opt => opt.MapFrom(src => src.Quotes.HighPrice))
                .ForMember(dest => dest.LowPrice, opt => opt.MapFrom(src => src.Quotes.LowPrice))
                .ForMember(dest => dest.OpenPrice, opt => opt.MapFrom(src => src.Quotes.OpenPrice))
                .ForMember(dest => dest.PreviousClosePrice, opt => opt.MapFrom(src => src.Quotes.PreviousClosePrice))
                .ForMember(dest => dest.Change, opt => opt.MapFrom(src => src.Quotes.Change))
                .ForMember(dest => dest.PercentChange, opt => opt.MapFrom(src => src.Quotes.PercentChange))
                .ForMember(dest => dest.Timestamp, opt => opt.MapFrom(src => src.Quotes.Timestamp));

                
        }
    }
}