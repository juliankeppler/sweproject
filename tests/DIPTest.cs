using System;
using System.Linq;
using System.Net;
using Xunit;
using sweproject;
using System.Reflection;
using System.Collections.Generic;

namespace tests {

    public class MockWebClient : IWebClient {

        public string downloadStringReturn { get; set; }
        public string downloadStringAddress { get; set; }

        public string DownloadString(string address) {
            downloadStringAddress = address;
            return downloadStringReturn;
        }

        public WebHeaderCollection Headers { get; set; } = new WebHeaderCollection();

    }
    public class DIPTest {

        public DIP dip = new DIP();
        static internal TReturn CallPrivateMethod<TInstance, TReturn>(TInstance instance, string methodName, object[] parameters) {
            Type type = instance.GetType();
            BindingFlags bindingAttr = BindingFlags.NonPublic | BindingFlags.Instance;
            MethodInfo method = type.GetMethod(methodName, bindingAttr);
            
            return (TReturn)method.Invoke(instance, parameters);
        }

        [Theory]
        [InlineData("Corona", new int[] {19}, "{'numFound': 35}", "https://search.dip.bundestag.de/search-api/v1/advanced/search?term=Corona&sort=datum_auf&f.aktivitaetsart_p=05Reden%2c+Wortmeldungen+im+Plenum&f.wahlperiode=19&rows=0", 35)]
        [InlineData("Inuit am Äquator", new int[] {19}, "{'numFound': 0}", "https://search.dip.bundestag.de/search-api/v1/advanced/search?term=Inuit+am+%c3%84quator&sort=datum_auf&f.aktivitaetsart_p=05Reden%2c+Wortmeldungen+im+Plenum&f.wahlperiode=19&rows=0", 0)]
        [InlineData("Krise", new int[] {}, "{'numFound': 300}", "https://search.dip.bundestag.de/search-api/v1/advanced/search?term=Krise&sort=datum_auf&f.aktivitaetsart_p=05Reden%2c+Wortmeldungen+im+Plenum&rows=0", 300)]
        [InlineData("Krise", null, "{'numFound': 300}", "https://search.dip.bundestag.de/search-api/v1/advanced/search?term=Krise&sort=datum_auf&f.aktivitaetsart_p=05Reden%2c+Wortmeldungen+im+Plenum&rows=0", 300)]
        public void TestGetResults(string term, int[] electionPeriods, string DIPResponse, string expectedQuery, int expectedResults) {

            MockWebClient wc = new MockWebClient();
            wc.downloadStringReturn = DIPResponse;
            dip = new DIP(wc);
            int actual;
            if (electionPeriods != null) {
                actual = dip.GetResults(term, electionPeriods);
            } else {
                actual = dip.GetResults(term);
            }

            Assert.Equal(expectedQuery, wc.downloadStringAddress);
            Assert.Equal(expectedResults, actual);

        }

        [Theory]
        [MemberData(nameof(TestGetRelevanceData))]
        public void TestGetRelevanceWithoutGetResults(string term, int[] electionPeriods, GroupMode mode, string DIPResponse, string expectedQuery, SortedDictionary<string, int> expectedResults) {

            MockWebClient wc = new MockWebClient();
            wc.downloadStringReturn = DIPResponse;
            dip = new DIP(wc);

            SortedDictionary<string, int> actual;
            if (electionPeriods != null) {
                actual = dip.GetRelevance(term, mode, electionPeriods);
            } else {
                actual = dip.GetRelevance(term, mode);
            }

            Assert.Equal(expectedQuery, wc.downloadStringAddress);
            Assert.Equal(expectedResults, actual);
        }

        [Theory]
        [MemberData(nameof(TestGetRelevanceData))]
        public void TestGetRelevanceAfterGetResults(string term, int[] electionPeriods, GroupMode mode, string DIPResponse, string expectedQuery, SortedDictionary<string, int> expectedResults) {

            MockWebClient wc = new MockWebClient();
            wc.downloadStringReturn = DIPResponse;
            dip = new DIP(wc);
            if (electionPeriods != null) {
                int n = dip.GetResults(term, electionPeriods);
            } else {
                int n = dip.GetResults(term);
            }

            SortedDictionary<string, int> actual;
            if (electionPeriods != null) {
                actual = dip.GetRelevance(term, mode, electionPeriods);
            } else {
                actual = dip.GetRelevance(term, mode);
            }

            Assert.Equal(expectedQuery, wc.downloadStringAddress);
            Assert.Equal(expectedResults, actual);
        }
        public static IEnumerable<object[]> TestGetRelevanceData => new List<object[]>{
            new object[] {
                "Corona",
                new int[] {19},
                GroupMode.Year,
                "{'numFound': 4, 'documents': [{'datum':'2019-11-20'},{'datum':'2019-12-22'},{'datum':'2020-12-20'},{'datum':'2021-04-03'}]}",
                "https://search.dip.bundestag.de/search-api/v1/advanced/search?term=Corona&sort=datum_auf&f.aktivitaetsart_p=05Reden%2c+Wortmeldungen+im+Plenum&f.wahlperiode=19&rows=500&start=0",
                new SortedDictionary<string, int> {
                    {"2018", 0}, {"2019", 2}, {"2020", 1}, {"2021", 1}
                }
            },
            new object[] {
                "Giraffe",
                new int[] {19},
                GroupMode.Month,
                "{'numFound': 4, 'documents': [{'datum':'2019-11-20'},{'datum':'2019-12-22'},{'datum':'2020-12-20'},{'datum':'2021-04-03'}]}",
                "https://search.dip.bundestag.de/search-api/v1/advanced/search?term=Giraffe&sort=datum_auf&f.aktivitaetsart_p=05Reden%2c+Wortmeldungen+im+Plenum&f.wahlperiode=19&rows=500&start=0",
                new SortedDictionary<string, int> {
                    {"2017-11", 0}, {"2017-12", 0}, {"2018-01", 0}, {"2018-02", 0}, {"2018-03", 0}, {"2018-04", 0}, {"2018-05", 0}, {"2018-06", 0}, 
                    {"2018-07", 0}, {"2018-08", 0}, {"2018-09", 0}, {"2018-10", 0}, {"2018-11", 0}, {"2018-12", 0}, {"2019-01", 0}, {"2019-02", 0}, 
                    {"2019-03", 0}, {"2019-04", 0}, {"2019-05", 0}, {"2019-06", 0}, {"2019-07", 0}, {"2019-08", 0}, {"2019-09", 0}, {"2019-10", 0}, 
                    {"2019-11", 1}, {"2019-12", 1}, {"2020-01", 0}, {"2020-02", 0}, {"2020-03", 0}, {"2020-04", 0}, {"2020-05", 0}, {"2020-06", 0}, 
                    {"2020-07", 0}, {"2020-08", 0}, {"2020-09", 0}, {"2020-10", 0}, {"2020-11", 0}, {"2020-12", 1}, {"2021-01", 0}, {"2021-02", 0}, 
                    {"2021-03", 0}, {"2021-04", 1}, {"2021-05", 0}, {"2021-06", 0}, {"2021-07", 0}, {"2021-08", 0}, {"2021-09", 0}
                }
            },
            new object[] {
                "Giraffe",
                null,
                GroupMode.Year,
                "{'numFound': 4, 'documents': [{'datum':'2019-11-20'},{'datum':'2019-12-22'},{'datum':'2020-12-20'},{'datum':'2021-04-03'}]}",
                "https://search.dip.bundestag.de/search-api/v1/advanced/search?term=Giraffe&sort=datum_auf&f.aktivitaetsart_p=05Reden%2c+Wortmeldungen+im+Plenum&rows=500&start=0",
                new SortedDictionary<string, int> {
                    {"1977", 0}, {"1978", 0}, {"1979", 0}, {"1980", 0}, {"1981", 0}, {"1982", 0}, {"1983", 0}, {"1984", 0}, {"2021", 1},
                    {"1985", 0}, {"1986", 0}, {"1987", 0}, {"1988", 0}, {"1989", 0}, {"1990", 0}, {"1991", 0}, {"1992", 0}, {"1993", 0}, 
                    {"1994", 0}, {"1995", 0}, {"1996", 0}, {"1997", 0}, {"1998", 0}, {"1999", 0}, {"2000", 0}, {"2001", 0}, {"2002", 0}, 
                    {"2003", 0}, {"2004", 0}, {"2005", 0}, {"2006", 0}, {"2007", 0}, {"2008", 0}, {"2009", 0}, {"2010", 0}, {"2011", 0}, 
                    {"2012", 0}, {"2013", 0}, {"2014", 0}, {"2015", 0}, {"2016", 0}, {"2017", 0}, {"2018", 0}, {"2019", 2}, {"2020", 1}, 
                    
                }
            },
        };

        [Fact]
        public void TestGetResultsEmptyStringThrowsException() {
            MockWebClient wc = new MockWebClient();
            dip = new DIP(wc);

            Exception ex = Assert.Throws<ArgumentException>(() => dip.GetResults(""));

            Assert.Equal("No search term provided!", ex.Message);
        }

        [Theory]
        [MemberData(nameof(TestFillZeroesData))]
        public void TestFillZeroes(SortedDictionary<string, int> dict, GroupMode mode, int[] electionPeriods, SortedDictionary<string, int> expected) {
            SortedDictionary<string, int> actual = CallPrivateMethod<DIP, SortedDictionary<string, int>>(dip, "FillZeroes", new object[]{dict, mode, electionPeriods});
            
            Assert.Equal(expected, actual);
        }

        public static IEnumerable<object[]> TestFillZeroesData => new List<object[]>{
            new object[] { 
                new SortedDictionary<string, int>(){
                    {"2017", 3},{"2021",4}
                }, 
                GroupMode.Year, 
                new int[] {19}, 
                new SortedDictionary<string, int>(){
                    {"2017", 3},{"2018",0},{"2019",0},{"2020",0},{"2021",4}
                }
            }, // Test GroupMode.Year over one electoral term
            new object[] { 
                new SortedDictionary<string, int>(){
                    {"2017-11", 3},{"2018-05",123}
                }, 
                GroupMode.Month, 
                new int[] {19}, 
                new SortedDictionary<string, int>(){
                    {"2017-11", 3},{"2017-12", 0},{"2018-01", 0},{"2018-02", 0},{"2018-03", 0},{"2018-04", 0},{"2018-05", 123},{"2018-06", 0},
                    {"2018-07", 0},{"2018-08", 0},{"2018-09", 0},{"2018-10", 0},{"2018-11", 0},{"2018-12", 0},{"2019-01", 0},{"2019-02", 0},
                    {"2019-03", 0},{"2019-04", 0},{"2019-05", 0},{"2019-06", 0},{"2019-07", 0},{"2019-08", 0},{"2019-09", 0},{"2019-10", 0},
                    {"2019-11", 0},{"2019-12", 0},{"2020-01", 0},{"2020-02", 0},{"2020-03", 0},{"2020-04", 0},{"2020-05", 0},{"2020-06", 0},
                    {"2020-07", 0},{"2020-08", 0},{"2020-09", 0},{"2020-10", 0},{"2020-11", 0},{"2020-12", 0},{"2021-01", 0},{"2021-02", 0},
                    {"2021-03", 0},{"2021-04", 0},{"2021-05", 0},{"2021-06", 0},{"2021-07", 0},{"2021-08", 0},{"2021-09", 0}
                }
            }, // Test GroupMode.Month over one electoral term
        };
    }
}
