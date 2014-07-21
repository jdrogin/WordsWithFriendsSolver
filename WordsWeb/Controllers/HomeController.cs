using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WordsLib;

namespace WordsWeb.Controllers
{
    public class HomeController : Controller
    {
        private WordsLookup wordsLookup;

        public HomeController()
        {
            this.wordsLookup = new WordsLookup();
            this.wordsLookup.Init();
        }

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult UploadFile(HttpPostedFileBase file)
        {
            if (file.ContentLength > 0)
            {
                var fileName = Path.GetFileName(file.FileName);
                var path = Path.Combine(Server.MapPath("~/App_Data/uploads"), fileName);
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                file.SaveAs(path);

                Board board = BoardOCR.OCR(path);
                LetterInfo[] hand = BoardOCR.HandOCR(path);

                System.IO.File.Delete(path);

                ViewBag.hand = hand;
                ViewBag.board = board;

                ViewBag.remainingLetters = LetterSet.GetRemaining(board, hand);

                BoardSolver solver = new BoardSolver(this.wordsLookup);
                List<Board> solvedBoards = solver.Solve(board, hand);

                ViewBag.solvedBoards = solvedBoards;
                ViewBag.solveTime = solver.TimePerSolve;
                ViewBag.iterations = solver.IterationsPerSolve;
            }

            return View("SolvedResult");
        }
    }
}